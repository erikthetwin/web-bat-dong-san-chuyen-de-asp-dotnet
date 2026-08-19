using webapp_demo.Data;

namespace webapp_demo.Services.ML;

public static class DatasetGenerator
{
    public static List<HousingData> Generate(int count = 1000)
    {
        var rnd = new Random(123);
        string[] types = { "Căn hộ", "Nhà riêng", "Đất", "Nhà mặt tiền", "Mặt bằng kinh doanh", "Biệt thự" };
        var rows = new List<HousingData>();
        for (int i = 0; i < count; i++)
        {
            var d = DbSeeder.Districts[rnd.Next(DbSeeder.Districts.Count)];
            var t = types[rnd.Next(types.Length)];
            float area = 40 + (float)(rnd.NextDouble() * 200);
            int bedrooms = rnd.Next(1, 6);
            int bathrooms = Math.Max(1, bedrooms - rnd.Next(0, 2));
            int floors = 1 + rnd.Next(0, 4);
            float facade = 4 + (float)(rnd.NextDouble() * 6);
            double typeFactor = t switch
            {
                "Biệt thự" => 1.6, "Nhà mặt tiền" => 1.8, "Đất" => 1.1, "Căn hộ" => 0.9, _ => 1.0
            };
            double basePrice = (double)d.PricePerM2 * area * typeFactor;
            basePrice *= (1 + 0.03 * bedrooms);
            basePrice *= (1 + 0.02 * floors);
            float noise = 1 + (float)((rnd.NextDouble() - 0.5) * 0.2);
            rows.Add(new HousingData
            {
                District = d.Name,
                PropertyType = t,
                Area = area,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                Floors = floors,
                FacadeWidth = facade,
                IsForRent = i % 6 == 0 ? 1 : 0,
                Label = (float)(basePrice * noise)
            });
        }
        return rows;
    }
}