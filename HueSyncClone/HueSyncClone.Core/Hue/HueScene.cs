using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace HueSyncClone.Hue
{
    /// <summary>
    /// Indicates scene.
    /// </summary>
    [DebuggerDisplay("{Id,nq}: {Name,nq}")]
    public class HueScene
    {
        /// <summary>
        /// Get or set the id of the scene being modified or created.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Get or set human readable name of the scene. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get the light ids which are in the scene. 
        /// </summary>
        public IList<string> Lights { get; } = new List<string>();

        /// <summary>
        /// Get or set whitelist user that created or modified the content of the scene.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Get or set whether the scene can be automatically deleted by the bridge.
        /// </summary>
        public bool Recycle { get; set; }

        /// <summary>
        /// Get or set the scene is locked by a rule or a schedule and cannot be deleted until all resources requiring or that reference the scene are deleted.
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object> AppData { get; set; }

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Picture { get; set; }

        /// <summary>
        /// Get or set UTC time the scene has been created or has been updated by a PUT.
        /// Will be null when unknown (legacy scenes).
        /// </summary>
        public DateTimeOffset? LastUpdated { get; set; }

        /// <summary>
        /// Version of scene document:
        /// 1 – Scene created via PUT, lightstates will be empty.
        /// 2 – Scene created via POST lightstates available.
        /// </summary>
        public int Version { get; set; }
    }
}