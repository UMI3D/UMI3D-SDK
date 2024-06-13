namespace umi3d.common.interaction.form
{
    public class ConnectionFormDto : FormDto
    {
        /// <summary>tar
        /// Globaltoken previously used in the media the client want to connect to.
        /// </summary>
        public string globalToken { get; set; }

        /// <summary>
        /// array that can be use to store data.
        /// </summary>
        public byte[] metadata { get; set; }
    }
}