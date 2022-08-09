using System;

namespace CentralOperativa.Domain.Cms.Forms
{
    [Serializable]
    public enum FieldType : byte
    {
        Label = 0,
        Text = 1,
        Date = 2,
        DropDownList = 3,
        RadioButtonList = 4,
        CheckBoxList = 5,
        Custom = 6
    }
}