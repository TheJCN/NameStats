using KnowledgePicker.WordCloud;
using KnowledgePicker.WordCloud.Coloring;
using KnowledgePicker.WordCloud.Drawing;
using KnowledgePicker.WordCloud.Layouts;
using KnowledgePicker.WordCloud.Primitives;
using KnowledgePicker.WordCloud.Sizers;
using SkiaSharp;

namespace NameStats;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                context.Response.Redirect("/index.html");
            });

            endpoints.MapGet("/chart", async context =>
            {
                var year = context.Request.Query["year"].FirstOrDefault();
                int? parsedYear = string.IsNullOrEmpty(year) ? null : int.Parse(year);

                var filteredData = parsedYear.HasValue
                    ? Program.NamesData.Where(d => d.BirthDate.Year == parsedYear.Value)
                    : Program.NamesData;

                var wordCloudData = filteredData
                    .GroupBy(d => d.Name)
                    .Select(group => new { Name = group.Key, Count = group.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(100)
                    .ToList();

                var imageBytes = GenerateWordCloud(wordCloudData, "Word Cloud");
                context.Response.ContentType = "image/png";
                await context.Response.Body.WriteAsync(imageBytes);
            });
        });
    }

    private static byte[] GenerateWordCloud(IEnumerable<dynamic> filteredData, string title)
    {
        var frequencies = filteredData
            .ToDictionary(d => (string)d.Name, d => (int)d.Count);

        var wordEntries = frequencies
            .Select(p => new WordCloudEntry(p.Key, p.Value));

        var wordCloud = new WordCloudInput(wordEntries)
        {
            Width = 1280,
            Height = 720,
            MinFontSize = 12,
            MaxFontSize = 42
        };

        var sizer = new LogSizer(wordCloud); 
        using var engine = new SkGraphicEngine(sizer, wordCloud);
        var layout = new SpiralLayout(wordCloud);
        var colorizer = new RandomColorizer();

        var wcg = new WordCloudGenerator<SKBitmap>(wordCloud, engine, layout, colorizer);
        var items = wcg.Arrange();

        using var final = new SKBitmap(wordCloud.Width, wordCloud.Height);
        using var canvas = new SKCanvas(final);

        canvas.Clear(SKColors.Empty);
        using var bitmap = wcg.Draw();
        canvas.DrawBitmap(bitmap, 0, 0);

        using var memoryStream = new MemoryStream();
        using var data = final.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(memoryStream);

        return memoryStream.ToArray();
    }
}
