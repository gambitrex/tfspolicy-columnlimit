using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace ColumnLimitPolicy
{
    [Serializable]
    public class ColumnLimitPolicy : PolicyBase
    {
        #region Fields

        [NonSerialized]
        IPendingCheckin _pendingCheckin;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this check-in policy has an editable configuration
        /// </summary>
        /// <value><c>true</c> if this instance can edit; otherwise, <c>false</c>.</value>
        public override bool CanEdit
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Validates the column limit before check-in"; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public override string Type
        {
            get { return "Column Limit Policy"; }
        }

        /// <summary>
        /// Gets the type description.
        /// </summary>
        /// <value>The type description.</value>
        public override string TypeDescription
        {
            get { return "This policy prevents users from checking in files that don't comply with your coding style guidelines."; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Dispose method unsubscribes to the event so we don't get into 
        /// scenarios that can create null ref exceptions
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _pendingCheckin.PendingChanges.CheckedPendingChangesChanged -= PendingChanges_CheckedPendingChangesChanged;
        }

        public override bool Edit(IPolicyEditArgs policyEditArgs)
        {
            SettingWindow window = new SettingWindow();
            window.Show();
            return true;
        }

        /// <summary>
        /// Evaluates the pending changes for policy violations
        /// </summary>
        /// <returns></returns>
        public override PolicyFailure[] Evaluate()
        {
            var failures = new List<PolicyFailure>();
            const String supportedSourceFileExtensions = "vb|cs";

            String regex = String.IsNullOrEmpty(Properties.Settings.Default.Regex) ?
                                supportedSourceFileExtensions : Properties.Settings.Default.Regex;
            int columnLimit = Properties.Settings.Default.MaxColumnSize > 0 ?
                Properties.Settings.Default.MaxColumnSize : 120;

            // process each file in the set of pending changes
            foreach (var pendingChange in PendingCheckin.PendingChanges.CheckedPendingChanges)
            {
                Match match = Regex.Match(Path.GetExtension(pendingChange.LocalItem), regex,
                    RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    int lineNumber = 0;
                    var reader = new StreamReader(new FileStream(pendingChange.LocalItem,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.ReadWrite));

                    while (!reader.EndOfStream)
                    {
                        lineNumber++;
                        string line = reader.ReadLine();
                        if (line.Length > columnLimit)
                        {
                            failures.Add(new PolicyFailure("File: " + pendingChange.FileName + " - Line: " + lineNumber +
                            "\nExceeded column limit (Column limit: " + columnLimit + ", Actual: " + line.Length + ").", this));
                        }
                    }
                }
            }
            return failures.ToArray();
        }

        /// <summary>
        /// Initializes this policy for the specified pending checkin.
        /// </summary>
        /// <param name="pendingCheckin" />The pending checkin.</param>
        public override void Initialize(IPendingCheckin pendingCheckin)
        {
            base.Initialize(pendingCheckin);

            _pendingCheckin = pendingCheckin;
            pendingCheckin.PendingChanges.CheckedPendingChangesChanged += PendingChanges_CheckedPendingChangesChanged;
        }

        /// <summary>
        /// Subscribes to the PendingChanges_CheckedPendingChangesChanged event 
        /// and reevaluates the policy. You should not do this if your policy takes 
        /// a long time to evaluate since this will get fired every time there is a
        /// change to one of the items in the pending changes window.
        /// </summary>
        /// <param name="sender" />The source of the event.</param>
        /// <param name="e" />The <see cref="System.EventArgs" /> instance containing the event data.</param>
        void PendingChanges_CheckedPendingChangesChanged(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                OnPolicyStateChanged(Evaluate());
            }
        }

        #endregion Methods
    }
}
