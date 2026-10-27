using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Logging.StructuredLogger
{
    public enum AssemblyResolutionSearchResult
    {
        Unknown,
        FileNotFound,
        FusionNamesDidNotMatch,
        TargetHadNoFusionName,
        NotInGac,
        NotAFileNameOnDisk,
        ProcessorArchitectureDoesNotMatch,
    }

    [Flags]
    internal enum AssemblyResolutionSearchAttemptContext
    {
        None = 0,
        SearchPathUnchanged = 1,
        ParentAssemblyUnchanged = 2,
        AssemblyFoldersExUnchanged = 4,
    }

    public sealed class AssemblyResolutionSearchAttempt
    {
        internal AssemblyResolutionSearchAttempt(
            string fileNameAttempted,
            string searchPath,
            string parentAssembly,
            string assemblyName,
            AssemblyResolutionSearchResult result,
            string processorArchitecture,
            bool isAssemblyFoldersExSearch)
        {
            FileNameAttempted = fileNameAttempted;
            SearchPath = searchPath;
            ParentAssembly = parentAssembly;
            AssemblyName = assemblyName;
            Result = result;
            ProcessorArchitecture = processorArchitecture;
            IsAssemblyFoldersExSearch = isAssemblyFoldersExSearch;
        }

        public string FileNameAttempted { get; }
        public string SearchPath { get; }
        public string ParentAssembly { get; }
        public string AssemblyName { get; }
        public AssemblyResolutionSearchResult Result { get; }
        public string ProcessorArchitecture { get; }
        public bool IsAssemblyFoldersExSearch { get; }

        internal AssemblyResolutionSearchAttemptContext GetUnchangedContext(AssemblyResolutionSearchAttempt previous)
        {
            AssemblyResolutionSearchAttemptContext result = AssemblyResolutionSearchAttemptContext.None;
            if (string.Equals(SearchPath, previous?.SearchPath, StringComparison.Ordinal)) result |= AssemblyResolutionSearchAttemptContext.SearchPathUnchanged;
            if (string.Equals(ParentAssembly, previous?.ParentAssembly, StringComparison.Ordinal)) result |= AssemblyResolutionSearchAttemptContext.ParentAssemblyUnchanged;
            if (IsAssemblyFoldersExSearch == (previous?.IsAssemblyFoldersExSearch ?? false)) result |= AssemblyResolutionSearchAttemptContext.AssemblyFoldersExUnchanged;
            return result;
        }
    }

    public sealed class AssemblyResolutionSearchTraceEventArgs : BuildMessageEventArgs
    {
        internal AssemblyResolutionSearchTraceEventArgs(
            string requestedAssemblyName,
            string targetProcessorArchitecture,
            IReadOnlyList<AssemblyResolutionSearchAttempt> searchAttempts,
            string senderName,
            MessageImportance importance,
            DateTime eventTimestamp)
            : base(null, null, senderName, importance, eventTimestamp)
        {
            RequestedAssemblyName = requestedAssemblyName;
            TargetProcessorArchitecture = targetProcessorArchitecture;
            SearchAttempts = searchAttempts;
        }

        public string RequestedAssemblyName { get; }
        public string TargetProcessorArchitecture { get; }
        public IReadOnlyList<AssemblyResolutionSearchAttempt> SearchAttempts { get; }

        public override string Message => string.Join(Environment.NewLine, SearchAttempts.Select(attempt =>
            $"{attempt.Result}: {attempt.FileNameAttempted} (search path: {attempt.SearchPath})"));
    }

    public enum AssemblyConflictLossReason
    {
        DidNotLose = 0,
        HadLowerVersion = 1,
        InsolubleConflict = 2,
        WasNotPrimary = 3,
        FusionEquivalentWithSameVersion = 4,
    }

    public sealed class AssemblyConflictDependee
    {
        internal AssemblyConflictDependee(string dependeeFullPath, IReadOnlyList<string> sourceItemSpecs)
        {
            DependeeFullPath = dependeeFullPath;
            SourceItemSpecs = sourceItemSpecs;
        }

        public string DependeeFullPath { get; }
        public IReadOnlyList<string> SourceItemSpecs { get; }
    }

    public sealed class AssemblyConflictReferenceDetails
    {
        internal AssemblyConflictReferenceDetails(
            string fusionName,
            string fullPath,
            bool isPrimary,
            bool isResolved,
            string unresolvedPrimaryItemSpec,
            IReadOnlyList<string> primarySourceItemSpecs,
            IReadOnlyList<AssemblyConflictDependee> dependees)
        {
            FusionName = fusionName;
            FullPath = fullPath;
            IsPrimary = isPrimary;
            IsResolved = isResolved;
            UnresolvedPrimaryItemSpec = unresolvedPrimaryItemSpec;
            PrimarySourceItemSpecs = primarySourceItemSpecs;
            Dependees = dependees;
        }

        public string FusionName { get; }
        public string FullPath { get; }
        public bool IsPrimary { get; }
        public bool IsResolved { get; }
        public string UnresolvedPrimaryItemSpec { get; }
        public IReadOnlyList<string> PrimarySourceItemSpecs { get; }
        public IReadOnlyList<AssemblyConflictDependee> Dependees { get; }
    }

    public sealed class AssemblyConflictDependencyDetailsMessageEventArgs : BuildMessageEventArgs
    {
        internal AssemblyConflictDependencyDetailsMessageEventArgs(
            AssemblyConflictReferenceDetails victor,
            AssemblyConflictReferenceDetails victim,
            string senderName,
            MessageImportance importance,
            DateTime eventTimestamp)
            : base(null, null, senderName, importance, eventTimestamp)
        {
            Victor = victor;
            Victim = victim;
        }

        public AssemblyConflictReferenceDetails Victor { get; }
        public AssemblyConflictReferenceDetails Victim { get; }
        public override string Message => $"Assembly conflict dependency details: {Victor.FusionName} wins over {Victim.FusionName}.";
    }

    public sealed class AssemblyConflictWarningEventArgs : BuildWarningEventArgs
    {
        internal AssemblyConflictWarningEventArgs(
            string simpleAssemblyName,
            AssemblyConflictLossReason lossReason,
            AssemblyConflictReferenceDetails victor,
            AssemblyConflictReferenceDetails victim,
            string code,
            string file,
            int lineNumber,
            int columnNumber,
            string helpKeyword,
            string senderName,
            DateTime eventTimestamp)
            : base(null, code, file, lineNumber, columnNumber, 0, 0,
                $"Found conflicts for {simpleAssemblyName}: {victor.FusionName} wins over {victim.FusionName} ({lossReason}).",
                helpKeyword, senderName, eventTimestamp)
        {
            SimpleAssemblyName = simpleAssemblyName;
            LossReason = lossReason;
            Victor = victor;
            Victim = victim;
        }

        public string SimpleAssemblyName { get; }
        public AssemblyConflictLossReason LossReason { get; }
        public AssemblyConflictReferenceDetails Victor { get; }
        public AssemblyConflictReferenceDetails Victim { get; }
    }
}
