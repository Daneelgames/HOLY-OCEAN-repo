#if MIRROR_43_0_OR_NEWER
using Mirror;

namespace twoloop
{
    public static class OffsetReaderWriter
    {
        public static void WriteOffset(this NetworkWriter writer, OriginShift.Offset value)
        {
            switch (OriginShift.singleton.precisionMode)
            {
                case OriginShift.OffsetPrecisionMode.Float:
                    writer.WriteFloat(value.vector.x);
                    writer.WriteFloat(value.vector.y);
                    writer.WriteFloat(value.vector.z);
                    break;
                
                case OriginShift.OffsetPrecisionMode.Double:
                    writer.WriteDouble(value.xDouble);
                    writer.WriteDouble(value.yDouble);
                    writer.WriteDouble(value.zDouble);
                    break;

                case OriginShift.OffsetPrecisionMode.Decimal:
                    writer.WriteDecimal(value.xDecimal);
                    writer.WriteDecimal(value.yDecimal);
                    writer.WriteDecimal(value.zDecimal);
                    break;
            }
        }

        public static OriginShift.Offset ReadOffset(this NetworkReader reader)
        {
            switch (OriginShift.singleton.precisionMode)
            {
                case OriginShift.OffsetPrecisionMode.Float:
                    return OriginShift.Offset.CreateWithFloat(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
                
                case OriginShift.OffsetPrecisionMode.Double:
                    return OriginShift.Offset.CreateWithDouble(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
                
                case OriginShift.OffsetPrecisionMode.Decimal:
                    return OriginShift.Offset.CreateWithDecimal(reader.ReadDecimal(), reader.ReadDecimal(), reader.ReadDecimal());
            }

            return new OriginShift.Offset();
        }
    }
}
#endif