using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.IO;
using System.Text.Encodings.Web;

namespace AspNetTips.MvcSite.TagHelpers
{
	[HtmlTargetElement(TagName, Attributes = AttributesName, TagStructure = TagStructure.WithoutEndTag)]
	public class ReplaceWebPTagHelper : UrlResolutionTagHelper
	{
		private const string TagName = "img";
		private const string AttributesName = SrcAttributeName + "," + ReplaceWebpAttributeName;
		private const string SrcAttributeName = "src";
		private const string ReplaceWebpAttributeName = "replace-webp";

		private readonly IWebHostEnvironment _env;

		public ReplaceWebPTagHelper(
			IWebHostEnvironment env,
			IUrlHelperFactory urlHelperFactory,
			HtmlEncoder htmlEncoder
		) : base(urlHelperFactory, htmlEncoder)
		{
			_env = env;
		}

		/// <inheritdoc />
		public override int Order => -1001;

		[HtmlAttributeName(SrcAttributeName)]
		public string Src { get; set; }

		[HtmlAttributeName(ReplaceWebpAttributeName)]
		public bool ReplaceWebp { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (output == null)
				throw new ArgumentNullException(nameof(output));

			output.CopyHtmlAttribute(SrcAttributeName, context);

			if (ReplaceWebp)
			{
				Src = output.Attributes[SrcAttributeName].Value as string;

				var f1 = false;
				if (ViewContext.HttpContext.Request.Headers.TryGetValue("accept", out var sv1))
				{
					f1 = sv1.ToString().Contains("image/webp");
				}

				var f2 = false;
				if (ViewContext.HttpContext.Request.Headers.TryGetValue("user-agent", out var sv2))
				{
					var ua = sv2.ToString();
					f2 = ua.Contains("AppleWebKit") && ua.Contains("Version/14.");
				}

				if (f1 | f2)
				{
					var virtualPath = Path.ChangeExtension(Src, "webp");

					var physicalPath = _env.MapWebRootPath(virtualPath);

					if (File.Exists(physicalPath))
						Src = virtualPath;
				}

				output.Attributes.SetAttribute(SrcAttributeName, Src);
			}
		}
	}
}
