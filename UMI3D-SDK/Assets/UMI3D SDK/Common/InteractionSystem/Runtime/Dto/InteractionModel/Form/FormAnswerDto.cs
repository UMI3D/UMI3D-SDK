using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.interaction.form
{
    public class FormAnswerDto
    {
        public ulong formId { get; set; }
        public ulong pageId { get; set; }
        public ulong submitId { get; set; }
        public bool isCancelation { get; set; }
        public bool isBack { get; set; }
        public List<string> inputs { get; set; }
    }
}