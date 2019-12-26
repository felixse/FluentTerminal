using System;
using System.Runtime.InteropServices;
using FluentTerminal.Models.Messages.Protobuf;
// ReSharper disable CommentTypo

namespace FluentTerminal.Models
{
    public static class Extensions
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct GuidULongConverter
        {
            [FieldOffset(0)] internal Guid Guid;
            [FieldOffset(0)] internal ulong GuidPart1;
            [FieldOffset(8)] internal ulong GuidPart2;
        }

        /// <summary>
        /// Converts <see cref="PbGuid"/> to <see cref="Guid"/>.
        /// </summary>
        /// <remarks>
        /// Protobuf doesn't have native GUID type, thus we've created <see cref="PbGuid"/> which is used for
        /// (de)serializing regular <see cref="Guid"/>.
        /// </remarks>
        /// <seealso cref="ToPbGuid"/>
        public static Guid ToGuid(this PbGuid pbGuid) => new GuidULongConverter
            {GuidPart1 = pbGuid.GuidPart1, GuidPart2 = pbGuid.GuidPart2}.Guid;

        /// <summary>
        /// Converts <see cref="Guid"/> to <see cref="PbGuid"/>.
        /// </summary>
        /// <inheritdoc cref="ToGuid" select="remarks"/>
        /// <seealso cref="ToGuid"/>
        public static PbGuid ToPbGuid(this Guid guid)
        {
            var converter = new GuidULongConverter {Guid = guid};

            return new PbGuid{GuidPart1 = converter.GuidPart1, GuidPart2 = converter.GuidPart2};
        }
    }
}
