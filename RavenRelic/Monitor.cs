using System;
using System.Linq;
using Raven.Abstractions.Connection;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Connection.Profiling;
using Raven.Client.Document;

namespace RavenRelic
{
    public class Monitor
    {
        public static void AttachTo(IDocumentStore store)
        {
            var docStorebase = store as DocumentStoreBase;
            if (docStorebase != null)
            {
                docStorebase.SessionCreatedInternal += TrackSession;
            }
            store.AfterDispose += AfterDispose;
            if (store.JsonRequestFactory != null)
            {
                store.JsonRequestFactory.ConfigureRequest += BeginRequest;
                store.JsonRequestFactory.LogRequest += EndRequest;
            }
        }

        private static void TrackSession(InMemoryDocumentSessionOperations obj)
        {
            NewRelic.Api.Agent.NewRelic.IncrementCounter("RavenDB/SessionCount");
        }

        private static void BeginRequest(object sender, WebRequestEventArgs e)
        {
            NewRelic.Api.Agent.NewRelic.IncrementCounter("RavenDB/RequestCount");
        }

        private static void EndRequest(object sender, RequestResultArgs e)
        {
            string queryName = e.Url.Split('?')[0];
            if (queryName.StartsWith("/databases", StringComparison.InvariantCultureIgnoreCase))
            {
                // Trim out the database prefix to the query
                queryName = "/" + String.Join("/", queryName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Skip(2));
            }
            NewRelic.Api.Agent.NewRelic.RecordResponseTimeMetric("RavenDB/Query" + queryName, (long)e.DurationMilliseconds);
        }

        private static void AfterDispose(object sender, EventArgs e)
        {
            var store = sender as DocumentStore;
            if (store != null)
            {
                store.SessionCreatedInternal -= TrackSession;
                store.AfterDispose -= AfterDispose;

                if (store.JsonRequestFactory != null)
                {
                    store.JsonRequestFactory.ConfigureRequest -= BeginRequest;
                    store.JsonRequestFactory.LogRequest -= EndRequest;
                }
            }
        }
    }
}