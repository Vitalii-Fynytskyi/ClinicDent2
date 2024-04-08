using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicDent2.RequestAnswers
{
    public class PutStagesRequestAnswer
    {
        public List<int> ConflictedStagesIds { get; set; }
        public string NewLastModifiedDateTime { get; set; }
    }
}
