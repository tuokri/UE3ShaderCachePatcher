﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UE3ShaderCachePatcher;

public class BaseDataModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
