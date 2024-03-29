﻿using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
	public enum ColumnEncodingKind
	{
		Direct = 0,
		Dictionary = 1,
		DirectV2 = 2,
		DictionaryV2 = 3
	}


	[ProtoContract]
    public class ColumnEncoding
    {
		[ProtoMember(1)] public ColumnEncodingKind Kind { get; set; }
		[ProtoMember(2)] public uint DictionarySize { get; set; }
    }
}
