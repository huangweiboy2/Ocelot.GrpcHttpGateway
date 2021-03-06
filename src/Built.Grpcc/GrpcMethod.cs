﻿using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Built.Grpcc
{
    public class GrpcMethod<TRequest, KResult> where TRequest : class, IMessage<TRequest> where KResult : class, IMessage<KResult>
    {
        private static ConcurrentDictionary<MethodDescriptor, Method<TRequest, KResult>> methods
            = new ConcurrentDictionary<MethodDescriptor, Method<TRequest, KResult>>();

        public static Method<TRequest, KResult> GetMethod(MethodDescriptor methodDescriptor)//Method<TRequest, KResult>
        {
            if (methods.TryGetValue(methodDescriptor, out Method<TRequest, KResult> method))
                return method;

            int mtype = 0;
            if (methodDescriptor.IsClientStreaming)
                mtype = 1;
            if (methodDescriptor.IsServerStreaming)
                mtype += 2;
            var methodType = (MethodType)Enum.ToObject(typeof(MethodType), mtype);

            var _method = new Method<TRequest, KResult>(methodType, methodDescriptor.Service.FullName
                , methodDescriptor.Name, ArgsParser<TRequest>.Marshaller, ArgsParser<KResult>.Marshaller);

            methods.TryAdd(methodDescriptor, _method);

            return _method;
        }
    }

    public static class ArgsParser<T> where T : class, IMessage<T>
    {
        public static MessageParser<T> Parser = new MessageParser<T>(() => Activator.CreateInstance<T>());
        public static Marshaller<T> Marshaller = Marshallers.Create((arg) => MessageExtensions.ToByteArray(arg), Parser.ParseFrom);
    }
}