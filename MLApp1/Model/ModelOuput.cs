using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLApp1;

public class ModelOutput
{
    public ModelOutput()
    {
        ImagePath = "";
        Label = "";
        PredictedLabel = "";
    }

    public string ImagePath { get; set; }

    public string Label { get; set; }

    public string PredictedLabel { get; set; }
}