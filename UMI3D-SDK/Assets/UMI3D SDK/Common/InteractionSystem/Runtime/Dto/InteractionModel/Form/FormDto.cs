using System.Collections.Generic;

namespace umi3d.common.interaction.form
{
    public class FormDto : ItemDto
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<PageDto> Pages { get; set; }

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