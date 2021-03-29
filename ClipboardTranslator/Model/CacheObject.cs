using MessagePack;

namespace ClipboardTranslator.Model
{
	[MessagePackObject]
	public class CacheObject
	{
		[Key(0)]
		public int Index
		{
			get;
			set;
		}

		[Key(1)]
		public string Key
		{
			get;
			set;
		}

		[Key(2)]
		public string Value
		{
			get;
			set;
		}

		public override string ToString()
		{
			return $"{Index}, {Key}, {Value}";
		}
	}
}
