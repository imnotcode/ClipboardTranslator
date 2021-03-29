using System;

using K4os.Compression.LZ4;

namespace ClipboardTranslator.Utility
{
	public static class Compression
	{
		public static byte[] Encode(byte[] source)
		{
			var target = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
			int encodedLength = LZ4Codec.Encode(source, 0, source.Length, target, 0, target.Length, LZ4Level.L12_MAX);

			var result = new byte[encodedLength];
			Buffer.BlockCopy(target, 0, result, 0, result.Length);

			return result;
		}

		public static byte[] Decode(byte[] source)
		{
			var target = new byte[source.Length * 255];
			int encodedLength = LZ4Codec.Decode(source, 0, source.Length, target, 0, target.Length);

			var result = new byte[encodedLength];
			Buffer.BlockCopy(target, 0, result, 0, result.Length);

			return result;
		}
	}
}
