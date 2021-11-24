using System;

namespace SE.AppConfiguration
{
    public abstract class ProgramOptions
    {
        public abstract bool Init();

        public abstract bool Execute();
    }
}