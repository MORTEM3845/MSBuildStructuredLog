namespace Microsoft.Build.Logging.StructuredLogger
{
    public sealed class AssemblyResolutionSearchTraceNode : Message
    {
        public string RequestedAssemblyName { get; set; }
        public string TargetProcessorArchitecture { get; set; }
        public override string TypeName => nameof(Message);
    }

    public sealed class AssemblyResolutionSearchAttemptNode : Message
    {
        public string FileNameAttempted { get; set; }
        public string SearchPath { get; set; }
        public string ParentAssembly { get; set; }
        public string AssemblyName { get; set; }
        public string Result { get; set; }
        public string ProcessorArchitecture { get; set; }
        public bool IsAssemblyFoldersExSearch { get; set; }
        public override string TypeName => nameof(Message);
    }

    public sealed class AssemblyConflictDependencyDetailsNode : Message
    {
        public string VictorFusionName { get; set; }
        public string VictorFullPath { get; set; }
        public string VictimFusionName { get; set; }
        public string VictimFullPath { get; set; }
        public override string TypeName => nameof(Message);
    }

    public sealed class AssemblyConflictWarningNode : Warning
    {
        public string SimpleAssemblyName { get; set; }
        public string LossReason { get; set; }
        public string VictorFusionName { get; set; }
        public string VictorFullPath { get; set; }
        public string VictimFusionName { get; set; }
        public string VictimFullPath { get; set; }
    }

    public sealed class AssemblyConflictReferenceNode : Message
    {
        public string Role { get; set; }
        public string FusionName { get; set; }
        public string FullPath { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsResolved { get; set; }
        public string UnresolvedPrimaryItemSpec { get; set; }
        public override string TypeName => nameof(Message);
    }

    public sealed class AssemblyConflictDependeeNode : Message
    {
        public string FullPath { get; set; }
        public override string TypeName => nameof(Message);
    }
}
