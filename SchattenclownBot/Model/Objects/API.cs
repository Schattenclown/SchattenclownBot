// Copyright (c) Schattenclown

using System;
using System.Collections.Generic;

using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects;

internal class API
{
	public int CommandRequestID { get; set; }
	public ulong RequestDiscordUserId { get; set; }
	public ulong RequestSecretKey { get; set; }
	public DateTime RequestTimeStamp { get; set; }
	public string RequesterIP { get; set; }
	public string Command { get; set; }
	public string Data { get; set; }
	internal static List<API> GET() => DB_API.GET();
	internal static void DELETE(int commandRequestID) => DB_API.DELETE(commandRequestID);
	public static void PUT(API aPI) => DB_API.PUT(aPI);
}
