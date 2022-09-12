using OldMusicBox.EIH.Client.Logging;
using OldMusicBox.EIH.Client.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Model
{
    /// <summary>
    /// Utility class used by the server to create a POST bound response
    /// </summary>
    public class SamlArtResponseFactory
    {
        /// <summary>
        /// Artifact returned to the client
        /// </summary>
        public string SAMLArt { get; set; }
        /// <summary>
        /// RelayState returned to the client
        /// </summary>
        public string RelayState { get; set; }

        /// <summary>
        /// Assertion Consumer Service URL (the client URL the SAML Art is to be returned to)
        /// </summary>
        public string Destination { get; set; }

        public string CreatePostBindingContent()
        {
            if ( string.IsNullOrEmpty( this.SAMLArt ) )
            {
                throw new ArgumentNullException( "SAMLArt", "SAMLArt cannot be null" );
            }
            if ( string.IsNullOrEmpty( this.Destination ) )
            {
                throw new ArgumentNullException( "Destination", "Destination cannot be null" );
            }

            string contentPage = new ResourceFactory().Create(ResourceFactory.EmbeddedResource.ArtifactPostResponse);

            contentPage = contentPage.Replace( "((Destination))", this.Destination );
            contentPage = contentPage.Replace( "((SAMLArt))", this.SAMLArt );
            contentPage = contentPage.Replace( "((RelayState))", this.RelayState );

            // log
            new LoggerFactory().For( this ).Debug( Event.ArtifactPostResponse, contentPage );

            return contentPage;
        }
    }
}
