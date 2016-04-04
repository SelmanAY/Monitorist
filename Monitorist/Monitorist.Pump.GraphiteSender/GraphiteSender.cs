using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Monitorist.Pump.Core;

namespace Monitorist.Pump.GraphiteSender
{
    public class GraphiteSender : ISender
    {
		public Config Settings { get; set; }

		public List<IDisposable> Links { get; set; }

		public BufferBlock<CounterValueModel> InputBlock { get; set; }

		public BatchBlock<CounterValueModel> BatchBlock { get; set; }
		
		public ActionBlock<CounterValueModel[]> BatchSenderBlock { get; set; }

		public ActionBlock<CounterValueModel> SenderBlock { get; set; }

		public void Initialize(string senderConfiguration, BufferBlock<CounterValueModel> inputBlock)
		{
			this.Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(senderConfiguration);
			this.InputBlock = inputBlock;

			this.BatchBlock = new BatchBlock<CounterValueModel>(this.Settings.BatchSize, new GroupingDataflowBlockOptions { Greedy = true });

			this.SenderBlock = new ActionBlock<CounterValueModel>(new Action<CounterValueModel>(Send), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 20 });
			this.BatchSenderBlock = new ActionBlock<CounterValueModel[]>(new Action<CounterValueModel[]>(Send), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 20 });

			this.Links = new List<IDisposable>();
			if (this.Settings.BatchSize > 1)
			{
				this.Links.Add(this.InputBlock.LinkTo(this.BatchBlock));
				this.Links.Add(this.BatchBlock.LinkTo(this.BatchSenderBlock));
			}
			else
			{
				this.Links.Add(this.InputBlock.LinkTo(this.SenderBlock));
			}
		}
		
		public void Send(CounterValueModel[] samples)
		{
			StringBuilder data = new StringBuilder();

			foreach (var s in samples)
			{
				foreach (var t in this.Settings.Transformations)
				{
					s.CounterName = System.Text.RegularExpressions.Regex.Replace(s.CounterName, t.Regex, t.Replace);
					s.CategoryName = System.Text.RegularExpressions.Regex.Replace(s.CategoryName, t.Regex, t.Replace);
					s.InstanceName = System.Text.RegularExpressions.Regex.Replace(s.InstanceName, t.Regex, t.Replace);
					s.HostName = System.Text.RegularExpressions.Regex.Replace(s.HostName, t.Regex, t.Replace);
				}

				var key = string.Join(".", new string[] {
					this.Settings.MetricPrefix,
					s.HostName,
					s.CategoryName,
					s.CounterName,
					string.IsNullOrEmpty(s.InstanceName) ? "one" : s.InstanceName
				});
				
				data.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1:0.000} {2}\n", key, s.Value, s.TimeStamp));
			}

			if (this.Settings.Protocol.Equals("UDP", StringComparison.InvariantCultureIgnoreCase))
			{
				var bytes = System.Text.Encoding.ASCII.GetBytes(data.ToString());
				using (var client = new System.Net.Sockets.UdpClient(this.Settings.Server, this.Settings.Port))
				{
					client.Send(bytes, bytes.Length);
				}
			}
			else if (this.Settings.Protocol.Equals("TCP", StringComparison.InvariantCultureIgnoreCase))
			{
				var bytes = System.Text.Encoding.ASCII.GetBytes(data.ToString());
				using (var client = new System.Net.Sockets.TcpClient())
				{
					client.Connect(this.Settings.Server, this.Settings.Port);
					using (var stream = client.GetStream())
					using (var writer = new System.IO.StreamWriter(stream))
					{
						writer.Write(data.ToString());
						writer.Flush();
					}
				}
			}
		}

		public void Send(CounterValueModel s)
		{

			foreach (var t in this.Settings.Transformations)
			{
				s.CounterName = System.Text.RegularExpressions.Regex.Replace(s.CounterName, t.Regex, t.Replace);
				s.CategoryName = System.Text.RegularExpressions.Regex.Replace(s.CategoryName, t.Regex, t.Replace);
				s.InstanceName = System.Text.RegularExpressions.Regex.Replace(s.InstanceName, t.Regex, t.Replace);
				s.HostName = System.Text.RegularExpressions.Regex.Replace(s.HostName, t.Regex, t.Replace);
			}

			var key = string.Join(".", new string[] {
				this.Settings.MetricPrefix,
				s.HostName,
				s.CategoryName,
				s.CounterName,
				string.IsNullOrEmpty(s.InstanceName) ? "one" : s.InstanceName
			});

			var data = string.Format(CultureInfo.InvariantCulture, "{0} {1:0.000} {2}\n", key, s.Value, s.TimeStamp);

			if (this.Settings.Protocol.Equals("UDP", StringComparison.InvariantCultureIgnoreCase))
			{
				var bytes = System.Text.Encoding.ASCII.GetBytes(data);
				using (var client = new System.Net.Sockets.UdpClient(this.Settings.Server, this.Settings.Port))
				{
					client.Send(bytes, bytes.Length);
				}
			}
			else if (this.Settings.Protocol.Equals("TCP", StringComparison.InvariantCultureIgnoreCase))
			{
				var bytes = System.Text.Encoding.ASCII.GetBytes(data);
				using (var client = new System.Net.Sockets.TcpClient())
				{
					client.Connect(this.Settings.Server, this.Settings.Port);
					using (var stream = client.GetStream())
					using (var writer = new System.IO.StreamWriter(stream))
					{
						writer.Write(data.ToString());
						writer.Flush();
					}
				}
			}
		}
    }
}
