﻿namespace AjLanguage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AjLanguage.Commands;
    using AjLanguage.Hosting;
    using AjLanguage.Language;
    using AjLanguage.Transactions;

    public class Machine
    {
        [ThreadStatic]
        private static FunctionStatus currentFunctionStatus;

        [ThreadStatic]
        private static Transaction currentTransaction;

        [ThreadStatic]
        private static Machine current;

        private BindingEnvironment environment = new BindingEnvironment();

        private Dictionary<Guid, IHost> localhosts = new Dictionary<Guid, IHost>();
        private Dictionary<Guid, IHost> remotehosts = new Dictionary<Guid, IHost>();
        private HashSet<Transaction> transactions = new HashSet<Transaction>();
        private HashSet<ITransactionalReference> snapshots = new HashSet<ITransactionalReference>();

        private TextReader inreader = System.Console.In;
        private TextWriter outwriter = System.Console.Out;
        private TextWriter errwriter = System.Console.Error;

        public Machine()
            : this(true)
        {
        }

        public Machine(bool iscurrent)
        {
            if (iscurrent)
                this.SetCurrent();

            this.Environment.SetValue("Machine", typeof(Machine));
        }

        public static FunctionStatus CurrentFunctionStatus
        {
            get
            {
                if (currentFunctionStatus == null)
                    currentFunctionStatus = new FunctionStatus();
                return currentFunctionStatus;
            }

            set
            {
                currentFunctionStatus = value;
            }
        }

        public static Transaction CurrentTransaction
        {
            get { return currentTransaction; }
            set { currentTransaction = value; }
        }

        public static Machine Current { get { return current; } }

        public IHost Host { get; set; }

        public BindingEnvironment Environment { get { return this.environment; } }

        public TextReader In
        {
            get
            {
                return this.inreader;
            }

            set
            {
                this.inreader = value;
            }
        }

        public TextWriter Out
        {
            get
            {
                return this.outwriter;
            }

            set
            {
                this.outwriter = value;
            }
        }

        public void RegisterHost(IHost host)
        {
            if (host.IsLocal)
                this.localhosts[host.Id] = host;
            else
                this.remotehosts[host.Id] = host;
        }

        public IHost GetHost(Guid id)
        {
            //if (this.Host != null && this.Host.Id == id)
            //    return this.Host;

            if (this.localhosts.ContainsKey(id))
                return this.localhosts[id];

            return this.remotehosts[id];
        }

        public ICollection<IHost> GetLocalHosts()
        {
            return this.localhosts.Values;
        }

        public ICollection<IHost> GetRemoteHosts()
        {
            return this.remotehosts.Values;
        }

        public void SetCurrent()
        {
            current = this;
        }

        public static void SetCurrent(Machine machine)
        {
            current = machine;
        }

        internal void RegisterTransaction(Transaction transaction)
        {
            lock (this.transactions)
            {
                this.transactions.Add(transaction);
            }
        }

        internal void UnregisterTransaction(Transaction transaction)
        {
            Transaction oldest = null;

            lock (this.transactions)
            {
                this.transactions.Remove(transaction);

                foreach (Transaction t in this.transactions)
                    if (oldest == null || Transaction.IsPrevious(t.Id, oldest.Id))
                        oldest = t;

                lock (this.snapshots)
                {
                    IList<ITransactionalReference> toremove = new List<ITransactionalReference>();

                    foreach (ITransactionalReference snapshot in this.snapshots)
                    {
                        if (oldest == null)
                            snapshot.ClearSnapshots();
                        else
                            snapshot.ClearSnapshots(oldest.Id);

                        if (!snapshot.HasSnapshots)
                            toremove.Add(snapshot);
                    }

                    foreach (ITransactionalReference reference in toremove)
                        snapshots.Remove(reference);

                    if (snapshots.Count == 0)
                        snapshots.Clear();
                }
            }
        }

        internal void RegisterSnapshot(ITransactionalReference reference)
        {
            lock (this.snapshots)
            {
                if (!this.snapshots.Contains(reference))
                    this.snapshots.Add(reference);
            }
        }
    }
}
