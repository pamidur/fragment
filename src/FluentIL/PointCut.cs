using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace FluentIL
{
    public class PointCut
    {
        private readonly ILProcessor _proc;
        private readonly Instruction _refInst;

        public ExtendedTypeSystem TypeSystem { get; }
        public MethodDefinition Method { get; }

        public PointCut(ILProcessor proc, Instruction instruction = null)
        {
            _proc = proc;
            _refInst = instruction;
            Method = proc.Body.Method;
            TypeSystem = Method.Module.GetTypeSystem();
        } 

        public PointCut Write(params Instruction[] instructions)
        {
            if (instructions.Length == 0)
                return this;

            var refi = _refInst;
            var startIndex = 0;

            if (refi == null)
            {
                if (_proc.Body.Instructions.Count != 0)
                    refi = _proc.SafeInsertBefore(_proc.Body.Instructions[0], instructions[0]);
                else
                    refi = _proc.SafeAppend(instructions[0]);

                startIndex = 1;
            }

            for (int i = startIndex; i < instructions.Length; i++)
            {
                Instruction inst = instructions[i];
                refi = _proc.SafeInsertAfter(refi, inst);
            }

            return new PointCut(_proc, refi);
        }

        public Instruction Emit(OpCode opCode, object operand)
        {
            switch (operand)
            {
                case TypeReference tr: return _proc.Create(opCode, TypeSystem.Import(tr));
                case MethodReference mr: return _proc.Create(opCode, TypeSystem.Import(mr));
                case CallSite cs: return _proc.Create(opCode, cs);
                case FieldReference fr: return _proc.Create(opCode, TypeSystem.Import(fr));
                case string str: return _proc.Create(opCode, str);
                case byte b: return _proc.Create(opCode, b);
                case sbyte sb: return _proc.Create(opCode, sb);
                case int i: return _proc.Create(opCode, i);
                case long l: return _proc.Create(opCode, l);
                case float f: return _proc.Create(opCode, f);
                case double d: return _proc.Create(opCode, d);
                case Instruction inst: return _proc.Create(opCode, inst);
                case Instruction[] insts: return _proc.Create(opCode, insts);
                case VariableDefinition vd: return _proc.Create(opCode, vd);
                case ParameterDefinition pd: return _proc.Create(opCode, pd);

                default: throw new NotSupportedException($"Not supported operand type '{operand.GetType()}'");
            }
        }

        public Instruction Emit(OpCode opCode)
        {
            return _proc.Create(opCode);
        }
    }

    public class PointCutOld
    {
        private readonly ILProcessor _proc;
        private readonly Instruction _refInst;

        public PointCutOld(ILProcessor proc, Instruction instruction)
        {
            _proc = proc;
            _refInst = instruction;
            TypeSystem = proc.Body.Method.Module.GetTypeSystem();
        }

        public ExtendedTypeSystem TypeSystem { get; }

        public PointCutOld Append(Instruction instruction)
        {
            _proc.SafeInsertBefore(_refInst, instruction);
            return this;
        }

        public virtual PointCutOld CreatePointCut(Instruction instruction)
        {
            return new PointCutOld(_proc, instruction);
        }

        public void Return()
        {
            Append(CreateInstruction(OpCodes.Ret));
        }

        public PointCutOld Call(MethodReference method, Action<PointCutOld> args = null)
        {
            args?.Invoke(this);

            var methodRef = _proc.Body.Method.MakeCallReference(TypeSystem.Import(method));
            var def = method.Resolve();

            var code = OpCodes.Call;

            if (def.IsConstructor)
                code = OpCodes.Newobj;
            else if (def.IsVirtual)
                code = OpCodes.Callvirt;

            var inst = _proc.Create(code, methodRef);
            Append(inst);

            return this;
        }

        public PointCutOld This()
        {
            if (_proc.Body.Method.HasThis)
                Append(CreateInstruction(OpCodes.Ldarg_0));
            else throw new Exception("Attempt to load 'this' on static method.");

            return this;
        }

        public PointCutOld ThisOrStatic()
        {
            if (_proc.Body.Method.HasThis)
                return This();

            return this;
        }

        public PointCutOld ThisOrNull()
        {
            if (_proc.Body.Method.HasThis)
                return This();
            else
                return Null();
        }

        public void Store(FieldReference field, Action<PointCutOld> val = null)
        {
            val?.Invoke(this);

            var fieldRef = _proc.Body.Method.MakeCallReference(TypeSystem.Import(field));
            //var fieldRef2 = _proc.Body.Method.ParametrizeGenericChild(_typeSystem.Import(field));

            var fieldDef = field.Resolve();

            Append(CreateInstruction(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldRef));
        }

        public void Store(VariableDefinition variable, Action<PointCutOld> val = null)
        {
            val?.Invoke(this);
            Append(CreateInstruction(OpCodes.Stloc, variable));
        }

        public void Store(ParameterReference par, Action<PointCut> val)
        {
            if (par.ParameterType.IsByReference)
            {
                Load(par);
                val.Invoke(this);
                Append(PickStoreInstruction(par.ParameterType));
            }
            else
            {
                val.Invoke(this);
                Append(CreateInstruction(OpCodes.Starg, _proc.Body.Method.HasThis ? par.Index + 1 : par.Index));
            }
        }

        private Instruction PickStoreInstruction(TypeReference parameterType)
        {
            var elemType = ((ByReferenceType)parameterType).ElementType;

            switch (elemType.MetadataType)
            {
                case MetadataType.Class: return CreateInstruction(OpCodes.Stind_Ref);
                case MetadataType.Object: return CreateInstruction(OpCodes.Stind_Ref);
                case MetadataType.MVar: return CreateInstruction(OpCodes.Stobj, elemType);
                case MetadataType.Var: return CreateInstruction(OpCodes.Stobj, elemType);
                case MetadataType.Double: return CreateInstruction(OpCodes.Stind_R8);
                case MetadataType.Single: return CreateInstruction(OpCodes.Stind_R4);
                case MetadataType.Int64: return CreateInstruction(OpCodes.Stind_I8);
                case MetadataType.UInt64: return CreateInstruction(OpCodes.Stind_I8);
                case MetadataType.Int32: return CreateInstruction(OpCodes.Stind_I4);
                case MetadataType.UInt32: return CreateInstruction(OpCodes.Stind_I4);
                case MetadataType.Int16: return CreateInstruction(OpCodes.Stind_I2);
                case MetadataType.UInt16: return CreateInstruction(OpCodes.Stind_I2);
                case MetadataType.Byte: return CreateInstruction(OpCodes.Stind_I1);
                case MetadataType.SByte: return CreateInstruction(OpCodes.Stind_I1);
                case MetadataType.Boolean: return CreateInstruction(OpCodes.Stind_I1);
                case MetadataType.Char: return CreateInstruction(OpCodes.Stind_I2);
                case MetadataType.UIntPtr: return CreateInstruction(OpCodes.Stind_I);
                case MetadataType.IntPtr: return CreateInstruction(OpCodes.Stind_I);
            }

            throw new NotSupportedException();
        }

        private FieldDefinition FindField(TypeDefinition type, string name)
        {
            if (type == null)
                return null;

            var field = type.Fields.FirstOrDefault(f => f.Name == name);
            return field ?? FindField(type.BaseType?.Resolve(), name);
        }

        private void InjectInitialization(MethodDefinition initMethod,
            FieldDefinition field,
            Action<PointCutOld> factory
            )
        {
            initMethod.GetEditor().OnEntry(
                e => e
                .If(
                    l => l.This().Load(field),
                    r => r.Null(),// (this.)aspect == null
                    pos => pos.This().Store(field, factory)// (this.)aspect = new aspect()
                )
            );
        }

        public PointCutOld Load(FieldReference field)
        {
            var fieldRef = _proc.Body.Method.MakeCallReference(TypeSystem.Import(field));
            var fieldDef = field.Resolve();

            Append(CreateInstruction(fieldDef.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldRef));

            return this;
        }

        public PointCutOld Load(MethodReference method)
        {
            Append(CreateInstruction(OpCodes.Ldftn, method));
            return this;
        }

        public PointCutOld Load(VariableDefinition variable)
        {
            Append(CreateInstruction(OpCodes.Ldloc, variable));
            return this;
        }

        public PointCutOld LoadRef(VariableDefinition variable)
        {
            Append(CreateInstruction(OpCodes.Ldloca, variable));
            return this;
        }

        public PointCutOld Pop()
        {
            Append(CreateInstruction(OpCodes.Pop));

            return this;
        }

        public PointCutOld Load(ParameterReference par)
        {
            var argIndex = _proc.Body.Method.HasThis ? par.Index + 1 : par.Index;
            Append(CreateInstruction(OpCodes.Ldarg, argIndex));
            return this;
        }     


        public PointCutOld If(Action<PointCutOld> left, Action<PointCutOld> right, Action<PointCutOld> pos = null, Action<PointCutOld> neg = null)
        {
            left(this);
            right(this);

            Append(CreateInstruction(OpCodes.Ceq));

            var continuePoint = CreateInstruction(OpCodes.Nop);
            var doIfTruePointCut = CreatePointCut(CreateInstruction(OpCodes.Nop));

            Append(CreateInstruction(OpCodes.Brfalse, continuePoint));
            Append(doIfTruePointCut._refInst);

            pos?.Invoke(doIfTruePointCut);

            if (neg != null)
            {
                var exitPoint = CreateInstruction(OpCodes.Nop);
                var doIfFlasePointCut = CreatePointCut(CreateInstruction(OpCodes.Nop));

                Append(CreateInstruction(OpCodes.Br, exitPoint));
                Append(continuePoint);
                Append(doIfFlasePointCut._refInst);

                neg(doIfFlasePointCut);

                Append(exitPoint);
            }
            else
            {
                Append(continuePoint);
            }

            return this;
        }
    }
}