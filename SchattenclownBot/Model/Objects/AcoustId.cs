// Copyright (c) Schattenclown

using System.Collections.Generic;

using Newtonsoft.Json;

namespace SchattenclownBot.Model.Objects;

internal class AcoustId
{
	public class Artist
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class Date
	{
		public int Day { get; set; }
		public int Month { get; set; }
		public int Year { get; set; }
	}

	public class Medium
	{
		public string Format { get; set; }
		public int Position { get; set; }
		public int TrackCount { get; set; }
		public List<Track> Tracks { get; set; }
	}

	public class Recording
	{
		public List<Artist> Artists { get; set; }
		public int Duration { get; set; }
		public string Id { get; set; }
		public List<Release> Releases { get; set; }
		public int Sources { get; set; }
		public string Title { get; set; }
		public List<ReleaseGroup> ReleaseGroups { get; set; }
	}

	public class ReleaseGroup
	{
		public string Id { get; set; }
		public List<Release> Releases { get; set; }
	}

	public class Release
	{
		public string Country { get; set; }
		public Date Date { get; set; }
		public string Id { get; set; }
		public int MediumCount { get; set; }
		public List<Medium> Mediums { get; set; }
		public List<ReleaseEvent> ReleaseEvents { get; set; }
		public string Title { get; set; }
		public int TrackCount { get; set; }
	}

	public class ReleaseEvent
	{
		public string Country { get; set; }
		public Date Date { get; set; }
	}

	public class Result
	{
		public string Id { get; set; }
		public List<Recording> Recordings { get; set; }
		public double Score { get; set; }
	}

	public class Root
	{
		public List<Result> Results { get; set; }
		public string Status { get; set; }
	}

	public class Track
	{
		public string Id { get; set; }
		public int Position { get; set; }
	}

	public static Root CreateObj(string content)
	{
		var lst = JsonConvert.DeserializeObject<Root>(content);

		if (lst == null)
			return null;

		Root obj = new()
		{
			Results = lst.Results,
			Status = lst.Status
		};

		return obj;
	}
}
