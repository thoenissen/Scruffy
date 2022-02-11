using System.IO;
using System.Net.Http;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.WebApi;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Scruffy.Services.Guild;

/// <summary>
/// Guild emblem management
/// </summary>
public class GuildEmblemService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Factory
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="httpClientFactory">Http client factory</param>
    public GuildEmblemService(LocalizationService localizationService, IHttpClientFactory httpClientFactory)
        : base(localizationService)
    {
        _httpClientFactory = httpClientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Post random guild emblems
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostRandomGuildEmblems(CommandContextContainer commandContext, int count)
    {
        var connector = new GuidWars2ApiConnector(null);
        await using (connector.ConfigureAwait(false))
        {
            var random = new Random(DateTime.Now.Millisecond);

            var foregrounds = await connector.GetGuildEmblemForegrounds()
                                             .ConfigureAwait(false);
            var backgrounds = await connector.GetGuildEmblemBackgrounds()
                                             .ConfigureAwait(false);

            var colors = new List<SixLabors.ImageSharp.Color>
                         {
                             new Rgba32(190, 186, 185),
                             new Rgba32(136, 0, 10),
                             new Rgba32(36, 65, 121),
                             new Rgba32(157, 143, 108),
                             new Rgba32(26, 24, 27),
                             new Rgba32(31, 88, 45),
                             new Rgba32(71, 69, 70),
                             new Rgba32(117, 25, 66),
                             new Rgba32(54, 5, 1),
                             new Rgba32(137, 51, 4),
                             new Rgba32(133, 36, 27),
                             new Rgba32(137, 85, 1),
                             new Rgba32(113, 72, 16),
                             new Rgba32(36, 79, 0),
                             new Rgba32(82, 69, 0),
                             new Rgba32(24, 39, 0),
                             new Rgba32(0, 108, 108),
                             new Rgba32(0, 55, 49),
                             new Rgba32(125, 136, 138),
                             new Rgba32(0, 78, 109),
                             new Rgba32(0, 31, 52),
                             new Rgba32(199, 93, 103),
                             new Rgba32(53, 53, 115),
                             new Rgba32(95, 29, 101),
                             new Rgba32(69, 39, 99),
                             new Rgba32(70, 16, 66),
                         };

            for (var i = 0; i < count; i++)
            {
                var fileName = new StringBuilder();

                fileName.Append("emblem_");

                var foregroundId = foregrounds[random.Next(0, foregrounds.Count - 1)];
                fileName.Append(foregroundId);

                var backgroundId = backgrounds[random.Next(0, backgrounds.Count - 1)];
                fileName.Append('_');
                fileName.Append(backgroundId);

                var foregroundLayers = await connector.GetGuildEmblemForegroundLayer(foregroundId)
                                                      .ConfigureAwait(false);
                var backgroundLayers = await connector.GetGuildEmblemBackgroundLayer(backgroundId)
                                                      .ConfigureAwait(false);

                var client = _httpClientFactory.CreateClient();

                SixLabors.ImageSharp.Image target = null;

                foreach (var layer in backgroundLayers.Layers)
                {
                    var response = await client.GetAsync(layer)
                                               .ConfigureAwait(false);

                    var stream = await response.Content
                                               .ReadAsStreamAsync()
                                               .ConfigureAwait(false);

                    await using (stream.ConfigureAwait(false))
                    {
                        stream.Position = 0;

                        stream.Position = 0;

                        var tempImage = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(stream)
                                                       .ConfigureAwait(false);

                        var colorIndex = random.Next(0, colors.Count - 1);
                        fileName.Append('_');
                        fileName.Append(colorIndex);

                        tempImage.Mutate(o => o.Fill(new RecolorBrush(new SixLabors.ImageSharp.Color(new Rgba32(176, 35, 33)), colors[colorIndex], 0.3f)));

                        if (target == null)
                        {
                            target = tempImage;
                        }
                        else
                        {
                            target.Mutate(o => o.DrawImage(tempImage, new Point(0, 0), 1f));
                        }
                    }
                }

                var isFlipHorizontal = random.Next(0, 10) <= 2;
                var isFlipVertical = random.Next(0, 10) <= 2;

                if (isFlipHorizontal)
                {
                    target.Mutate(o => o.Flip(FlipMode.Horizontal));
                }

                if (isFlipVertical)
                {
                    target.Mutate(o => o.Flip(FlipMode.Vertical));
                }

                isFlipHorizontal = random.Next(0, 10) <= 2;
                isFlipVertical = random.Next(0, 10) <= 2;

                foreach (var layer in foregroundLayers.Layers
                                                      .AsEnumerable()
                                                      .Skip(1))
                {
                    var response = await client.GetAsync(layer)
                                               .ConfigureAwait(false);

                    var stream = await response.Content
                                               .ReadAsStreamAsync()
                                               .ConfigureAwait(false);

                    await using (stream.ConfigureAwait(false))
                    {
                        stream.Position = 0;

                        var tempImage = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(stream)
                                                       .ConfigureAwait(false);

                        if (layer != foregroundLayers.Layers.First())
                        {
                            var colorIndex = random.Next(0, colors.Count - 1);
                            fileName.Append('_');
                            fileName.Append(colorIndex);

                            tempImage.Mutate(o => o.Fill(new GraphicsOptions
                                                         {
                                                             AlphaCompositionMode = PixelAlphaCompositionMode.SrcIn,
                                                         },
                                                         new RecolorBrush(SixLabors.ImageSharp.Color.White, colors[colorIndex], 0.26f)));
                        }

                        if (isFlipHorizontal)
                        {
                            tempImage.Mutate(o => o.Flip(FlipMode.Horizontal));
                        }

                        if (isFlipVertical)
                        {
                            tempImage.Mutate(o => o.Flip(FlipMode.Vertical));
                        }

                        target.Mutate(o => o.DrawImage(tempImage, new Point(0, 0), 1f));
                    }
                }

                var memoryStream = new MemoryStream();

                await using (memoryStream.ConfigureAwait(false))
                {
                    await target.SaveAsPngAsync(memoryStream)
                                .ConfigureAwait(false);

                    memoryStream.Position = 0;

                    fileName.Append(".png");

                    await commandContext.Channel
                                        .SendFileAsync(new FileAttachment(memoryStream, fileName.ToString()))
                                        .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // Methods
}