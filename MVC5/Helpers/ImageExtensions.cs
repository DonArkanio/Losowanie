using System.Web.Mvc;
using System.Web.Security;

namespace MVC5.Helpers
{
    public static class ImageExtensions
    {
        // HTML helper do wyciagania gravatara
        public static MvcHtmlString RenderGravatarImage(this HtmlHelper helper, string emailId, int imgSize)
        {
            // przekazujemy email uzytkownika z malych liter
            emailId = emailId.ToLower();

            // musimy zdobyc hash znajdujacy sie w linku
            var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(emailId, "MD5").ToLower();

            // budujemy ostateczny adres URL do naszego gravatra ze strony gravatar.com
            var imageUrl = string.Format(@"<img src=""http://www.gravatar.com/avatar/{0}?s={1}&r=pg"" />", hash, imgSize);

            // zwracamy sciezke do gravatara
            return new MvcHtmlString(imageUrl);
        }
    }
}