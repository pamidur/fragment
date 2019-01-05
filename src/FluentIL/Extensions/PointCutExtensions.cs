using Mono.Cecil.Cil;

namespace FluentIL.Extensions
{
    public static class PointCutExtensions
    {
        public static PointCut Write(this PointCut pc, OpCode opCode, object operand) => pc.Write(pc.Emit(opCode, operand));
        public static PointCut Write(this PointCut pc, OpCode opCode) => pc.Write(pc.Emit(opCode));
    }
}
