using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace FluentIL
{
    public static class Values
    {
        public static PointCut Null(this PointCut pc)
        {
            return pc.Write(OpCodes.Ldnull);
        }

        public static PointCut Dup(this PointCut pc)
        {
            return pc.Write(OpCodes.Dup);
        }

        public static PointCut Value(this PointCut pc, object value)
        {
            if (value == null)
                return Null(pc);

            var valueType = value.GetType();

            if (value is CustomAttributeArgument argument)
                return AttributeArgument(pc, argument);
            else if (value is TypeReference tr)
                return TypeOf(pc, tr);
            else if (valueType.IsValueType)
                return Primitive(pc, value);
            else if (value is string str)
                return pc.Write(OpCodes.Ldstr, str);
            //else if (valueType.IsArray)
            //    CreateArray(_typeSystem.Import(valueType.GetElementType()), il => ((Array)value).Cast<object>().Select(Value).ToArray());
            else
                throw new NotSupportedException(valueType.ToString());
        }

        public static PointCut TypeOf(this PointCut pc, TypeReference type)
        {
            return pc
                .Write(OpCodes.Ldtoken, pc.Method.MakeCallReference(type))
                .Write(OpCodes.Call, pc.TypeSystem.GetTypeFromHandleMethod);
        }

        public static PointCut MethodOf(this PointCut pc, MethodReference method)
        {
            return pc
                .Write(OpCodes.Ldtoken, method)
                .Write(OpCodes.Ldtoken, method.DeclaringType.MakeCallReference(method.DeclaringType))
                .Write(OpCodes.Call, pc.TypeSystem.GetMethodFromHandleMethod)
                ;
        }

        public static PointCut Primitive(this PointCut pc, object value)
        {
            var valueType = value.GetType();

            switch (value)
            {
                case bool bo: return pc.Write(bo ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                case long l: return pc.Write(OpCodes.Ldc_I8, l);
                case ulong ul: return pc.Write(OpCodes.Ldc_I8, unchecked((long)ul));
                case double d: return pc.Write(OpCodes.Ldc_R8, d);
                case int i: return pc.Write(OpCodes.Ldc_I4, i);
                case uint ui: return pc.Write(OpCodes.Ldc_I4, unchecked((int)ui));
                case float fl: return pc.Write(OpCodes.Ldc_R4, fl);
                case sbyte sb: return pc.Write(OpCodes.Ldc_I4, sb);
                case byte b: return pc.Write(OpCodes.Ldc_I4, b);
                case ushort us: return pc.Write(OpCodes.Ldc_I4, us);
                case short s: return pc.Write(OpCodes.Ldc_I4, s);
                case char c: return pc.Write(OpCodes.Ldc_I4, c);

                default: throw new NotSupportedException(valueType.ToString());
            }
        }

        public static PointCut Cast(this PointCut pc, TypeReference typeOnStack, TypeReference expectedType)
        {
            if (typeOnStack.Match(expectedType))
                return pc;

            if (expectedType.IsByReference)
                expectedType = ((ByReferenceType)expectedType).ElementType;

            if (typeOnStack.Match(expectedType))
                return pc;

            if (typeOnStack.IsByReference)
            {
                typeOnStack = ((ByReferenceType)typeOnStack).ElementType;
                pc = LoadFromReference(pc, typeOnStack);
            }

            if (typeOnStack.Match(expectedType))
                return pc;

            if (expectedType.IsValueType || expectedType.IsGenericParameter)
            {
                if (!typeOnStack.IsValueType)
                    return pc.Write(OpCodes.Unbox_Any, expectedType);
            }
            else
            {
                if (typeOnStack.IsValueType || typeOnStack.IsGenericParameter)
                    return pc.Write(OpCodes.Box, typeOnStack);
                else if (!expectedType.Match(pc.TypeSystem.Object))
                    return pc.Write(OpCodes.Castclass, expectedType);
            }

            throw new Exception($"Cannot cast '{typeOnStack}' to '{expectedType}'");
        }

        private static PointCut LoadFromReference(PointCut pc, TypeReference elemType)
        {
            switch (elemType.MetadataType)
            {
                case MetadataType.Class: return pc.Write(OpCodes.Ldind_Ref);
                case MetadataType.Object: return pc.Write(OpCodes.Ldind_Ref);
                case MetadataType.String: return pc.Write(OpCodes.Ldind_Ref);
                case MetadataType.MVar: return pc.Write(OpCodes.Ldobj, elemType);
                case MetadataType.Var: return pc.Write(OpCodes.Ldobj, elemType);
                case MetadataType.Double: return pc.Write(OpCodes.Ldind_R8);
                case MetadataType.Single: return pc.Write(OpCodes.Ldind_R4);
                case MetadataType.Int64: return pc.Write(OpCodes.Ldind_I8);
                case MetadataType.UInt64: return pc.Write(OpCodes.Ldind_I8);
                case MetadataType.Int32: return pc.Write(OpCodes.Ldind_I4);
                case MetadataType.UInt32: return pc.Write(OpCodes.Ldind_U4);
                case MetadataType.Int16: return pc.Write(OpCodes.Ldind_I2);
                case MetadataType.UInt16: return pc.Write(OpCodes.Ldind_U2);
                case MetadataType.Byte: return pc.Write(OpCodes.Ldind_U1);
                case MetadataType.SByte: return pc.Write(OpCodes.Ldind_I1);
                case MetadataType.Boolean: return pc.Write(OpCodes.Ldind_U1);
                case MetadataType.Char: return pc.Write(OpCodes.Ldind_U2);
                case MetadataType.UIntPtr: return pc.Write(OpCodes.Ldind_I);
                case MetadataType.IntPtr: return pc.Write(OpCodes.Ldind_I);
            }

            throw new NotSupportedException();
        }

        private static PointCut AttributeArgument(PointCut pc, CustomAttributeArgument argument)
        {
            var val = argument.Value;

            if (val.GetType().IsArray)
                pc = pc.CreateArray(
                    pc.TypeSystem.Import(argument.Type.GetElementType()),
                    ((Array)val).Cast<object>().Select<object, Action<PointCut>>(v => il => il.Value(v)).ToArray()
                    );
            else
            {
                pc = pc.Value(val);

                if (val is CustomAttributeArgument next)
                    pc = pc.Cast(next.Type, argument.Type);
            }

            return pc;
        }
    }
}
