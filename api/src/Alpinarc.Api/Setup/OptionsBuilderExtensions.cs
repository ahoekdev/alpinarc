using System.Reflection;
using Microsoft.Extensions.Options;

namespace Alpinarc.Api.Setup;

public static class OptionsBuilderExtensions
{
    extension<TOptions>(OptionsBuilder<TOptions> options) where TOptions : class
    {
        /// <summary>
        /// Validates the options' data annotations when the app starts, so a misconfigured deploy
        /// fails right away instead of on the first request. Skipped while the build generates the
        /// OpenAPI document: that starts the app too, without any configuration.
        /// </summary>
        public OptionsBuilder<TOptions> ValidateOnStartUnlessGeneratingOpenApi() =>
            Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider"
                ? options
                : options.ValidateDataAnnotations().ValidateOnStart();
    }
}
