<Query Kind="Program" />

void Main()
{
}

public class Test
{
	private readonly IDictionary<string, string> _items;

	private IReadOnlyList<string> Keys = [
		"Key000",
		"Key001",
		"Key002",
		"Key003",
		"Key004",
		"Key005",
		"Key006",
		"Key007",
		"Key008",
		"Key009",
		"Key010",
		"Key011",
		"Key012",
		"Key013",
		"Key014",
		"Key015",
		"Key016",
		"Key017",
		"Key018",
		"Key019",
		"Key020",
		"Key021",
		"Key022",
		"Key023",
		"Key024",
		"Key025",
		"Key026",
		"Key027",
		"Key028",
		"Key029",
		"Key030",
		"Key031",
		"Key032",
		"Key033",
		"Key034",
		"Key035",
		"Key036",
		"Key037",
		"Key038",
		"Key039",
		"Key040",
		"Key041",
		"Key042",
		"Key043",
		"Key044",
		"Key045",
		"Key046",
		"Key047",
		"Key048",
		"Key049",
		"Key050",
		"Key051",
		"Key052",
		"Key053",
		"Key054",
		"Key055",
		"Key056",
		"Key057",
		"Key058",
		"Key059",
		"Key060",
		"Key061",
		"Key062",
		"Key063",
		"Key064",
		"Key065",
		"Key066",
		"Key067",
		"Key068",
		"Key069",
		"Key070",
		"Key071",
		"Key072",
		"Key073",
		"Key074",
		"Key075",
		"Key076",
		"Key077",
		"Key078",
		"Key079",
		"Key080",
		"Key081",
		"Key082",
		"Key083",
		"Key084",
		"Key085",
		"Key086",
		"Key087",
		"Key088",
		"Key089",
		"Key090",
		"Key091",
		"Key092",
		"Key093",
		"Key094",
		"Key095",
		"Key096",
		"Key097",
		"Key098",
		"Key099"
	];
	
	public Test()
	{
		_items = new Dictionary<string, string> { 
			{ "Key001", "01" },
			{ "Key010", "02" },
			{ "Key020", "03" },
			{ "Key030", "04" },
			{ "Key040", "05" },
			{ "Key050", "06" },
			{ "Key060", "07" },
			{ "Key070", "08" },
			{ "Key080", "09" },
			{ "Key090", "10" },
		};
	}
	
	public IDictionary<string, string> FillFirstThenAdd(IDictionary<string, string> data)
	{
	    var result = new SortedDictionary<string, string>();

	    // fill result with data from fetchers
	    foreach (var item in data.Where(x => Keys.Contains(x.Key)))
	    {
	        result[item.Key] = item.Value;
	    }

	    // add missing keys with default values
	    foreach (var key in Keys)
	    {
	        if (!result.ContainsKey(key))
	        {
	            result[key] = string.Empty;
	        }
	    }

	    return result;
	}

	public IDictionary<string, string> AddFirstThenFill(IDictionary<string, string> data)
	{
	    var result = new SortedDictionary<string, string>();

	    // add all keys with default values
	    foreach (var key in Keys)
	    {
	        result[key] = string.Empty;
	    }

	    // fill result with data from fetchers
	    foreach (var item in data.Where(x => Keys.Contains(x.Key)))
	    {
	        result[item.Key] = item.Value;
	    }
		
	    return result;
	}
}
