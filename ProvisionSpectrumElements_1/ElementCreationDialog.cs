// ignore spelling: Dma

namespace ProvisionSpectrumElements_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using static ProvisionSpectrumElements_1.Script;

	using ElementStateIs = Skyline.DataMiner.Core.DataMinerSystem.Common.ElementState;

	public class ElementCreationDialog : Dialog
	{
		private readonly IEngine engine;

		public ElementCreationDialog(Engine engine) : base(engine)
		{
			this.engine = engine;
			var dms = engine.GetDms();

			Title = "Spectrum Element Provisioning";
			CancelButton = new Button("Cancel");
			CancelButton.Pressed += CancelButton_Pressed;
			ProvisionButton = new Button("Provision");
			ProvisionButton.Pressed += ProvisionButton_Pressed;
			ExitButton = new Button("Exit");
			ExitButton.Pressed += ExitButton_Pressed;
			AddWidget(CancelButton, 20, 0);
			AddWidget(ProvisionButton, 20, 1);

			// Community Section
			CommunitySection = new Section();

			CommunityLabel = new Label("Community Strings") { Style = TextStyle.Bold };
			GetCommunityLabel = new Label("Get Community String");
			SetCommunityLabel = new Label("Set Community String");

			GetCommunityText = new TextBox();
			SetCommunityText = new TextBox();
			GetCommunityText.Text = "public";
			SetCommunityText.Text = "private";

			CommunitySection.AddWidget(CommunityLabel, 0, 0);
			CommunitySection.AddWidget(GetCommunityLabel, 1, 0);
			CommunitySection.AddWidget(GetCommunityText, 1, 1);
			CommunitySection.AddWidget(SetCommunityLabel, 2, 0);
			CommunitySection.AddWidget(SetCommunityText, 2, 1);

			AddSection(CommunitySection, new SectionLayout(0, 0));

			//// Provision View

			ViewSection = new Section();

			ViewSectionLabel = new Label("Provisioning Location") { Style = TextStyle.Bold };
			NewViewCheckBox = new CheckBox("Different View");
			NewViewCheckBox.Checked += NewViewCheckBox_Checked;
			SameViewCheck = new CheckBox("Same View as CCAP elements");
			SameViewCheck.Checked += SameViewCheckBox_Checked;
			SameViewCheck.IsChecked = true;
			NewViewLabel = new Label("           View Name: ");
			ViewTextBox = new TextBox();

			ViewSection.AddWidget(ViewSectionLabel, 0, 0);
			ViewSection.AddWidget(SameViewCheck, 1, 0);
			ViewSection.AddWidget(NewViewCheckBox, 2, 0);
			ViewSection.AddWidget(NewViewLabel, 3, 0);
			ViewSection.AddWidget(ViewTextBox, 3, 1);

			AddSection(ViewSection, new SectionLayout(3, 0));

			// DMA Location
			TargetDmaSection = new Section();

			TargetDmaSectionLabel = new Label("Target DMA") { Style = TextStyle.Bold };
			SameDmaCheckBox = new CheckBox("Same DMA as Host")
			{
				IsChecked = true,
			};
			LoadBalancedDmaCheckBox = new CheckBox("Load Balanced");
			LoadBalancedDmaTextBox = new TextBox();
			SpecifiedDmaCheckBox = new CheckBox("Single DMA");
			SpecifiedDmaDropDown = new DropDown(dms.GetAgents().Select(x => x.HostName));

			SameDmaCheckBox.Checked += SameDMACheckBox_Checked;
			LoadBalancedDmaCheckBox.Checked += LoadBalancedDMACheckBox_Checked;
			SpecifiedDmaCheckBox.Checked += SpecifiedDMACheckBox_Checked;

			TargetDmaSection.AddWidget(TargetDmaSectionLabel, 0, 0);
			TargetDmaSection.AddWidget(SameDmaCheckBox, 1, 0);
			TargetDmaSection.AddWidget(LoadBalancedDmaCheckBox, 2, 0);
			TargetDmaSection.AddWidget(LoadBalancedDmaTextBox, 3, 1);
			TargetDmaSection.AddWidget(SpecifiedDmaCheckBox, 4, 0);
			TargetDmaSection.AddWidget(SpecifiedDmaDropDown, 4, 1);

			AddSection(TargetDmaSection, new SectionLayout(8, 0));

			ResultsPage = new Section();
			ExpectedElementsCreated = new Label("Expected Elements to Create:");
			ActualElementsCreated = new Label("Successfully Created Elements");
			Separator = new Label("--------------------------------");
			Details = new Label("Details:");
			Result = new TextBox { IsReadOnly = true, IsMultiline = true };
			ResultsPage.AddWidget(ExpectedElementsCreated, 0, 0);
			ResultsPage.AddWidget(ActualElementsCreated, 1, 0);
			ResultsPage.AddWidget(Separator, 2, 0);
			ResultsPage.AddWidget(Details, 3, 0);
			ResultsPage.AddWidget(Result, 4, 0);
			ResultsPage.AddWidget(ExitButton, 5, 0);
			AddSection(ResultsPage, new SectionLayout(8, 0));
			ResultsPage.IsVisible = false;
		}

		#region Properties

		public Section CommunitySection { get; set; }

		public Label CommunityLabel { get; set; }

		public Label GetCommunityLabel { get; set; }

		public TextBox GetCommunityText { get; set; }

		public Label SetCommunityLabel { get; set; }

		public TextBox SetCommunityText { get; set; }

		public Section ViewSection { get; set; }

		public Label ViewSectionLabel { get; set; }

		public CheckBox SameViewCheck { get; set; }

		public CheckBox NewViewCheckBox { get; set; }

		public Label NewViewLabel { get; set; }

		public TextBox ViewTextBox { get; set; }

		public Section TargetDmaSection { get; set; }

		public Label TargetDmaSectionLabel { get; set; }

		public CheckBox SameDmaCheckBox { get; set; }

		public CheckBox LoadBalancedDmaCheckBox { get; set; }

		public TextBox LoadBalancedDmaTextBox { get; set; }

		public CheckBox SpecifiedDmaCheckBox { get; set; }

		public DropDown SpecifiedDmaDropDown { get; set; }

		public Section ResultsPage { get; set; }

		public Label ExpectedElementsCreated { get; set; }

		public Label ActualElementsCreated { get; set; }

		public Label Separator { get; set; }

		public Label Details { get; set; }

		public TextBox Result { get; set; }

		public Button CancelButton { get; set; }

		public Button ExitButton { get; set; }

		public Button ProvisionButton { get; set; }

		private StringBuilder ResultBuilder { get; set; }

		#endregion

		private void SpecifiedDMACheckBox_Checked(object sender, System.EventArgs e)
		{
			SameDmaCheckBox.IsChecked = false;
			LoadBalancedDmaCheckBox.IsChecked = false;
			LoadBalancedDmaTextBox.IsEnabled = false;
			SpecifiedDmaDropDown.IsEnabled = true;
		}

		private void LoadBalancedDMACheckBox_Checked(object sender, System.EventArgs e)
		{
			SameDmaCheckBox.IsChecked = false;
			SpecifiedDmaCheckBox.IsChecked = false;
			LoadBalancedDmaTextBox.IsEnabled = true;
			SpecifiedDmaDropDown.IsEnabled = false;
		}

		private void SameDMACheckBox_Checked(object sender, System.EventArgs e)
		{
			LoadBalancedDmaCheckBox.IsChecked = false;
			SpecifiedDmaCheckBox.IsChecked = false;
			LoadBalancedDmaTextBox.IsEnabled = false;
			SpecifiedDmaDropDown.IsEnabled = false;
		}

		private void NewViewCheckBox_Checked(object sender, System.EventArgs e)
		{
			SameViewCheck.IsChecked = false;
			ViewTextBox.IsEnabled = true;
		}

		private void SameViewCheckBox_Checked(object sender, System.EventArgs e)
		{
			NewViewCheckBox.IsChecked = false;
			ViewTextBox.IsEnabled = false;
		}

		private bool ValidateInput()
		{
			bool isValid = true;
			var dms = engine.GetDms();

			// Validate sections
			isValid &= ValidateDifferentView(dms);
			isValid &= ValidateLoadBalancedDma(dms);

			return isValid;
		}

		private bool ValidateDifferentView(IDms dms)
		{
			if (!NewViewCheckBox.IsChecked)
			{
				return true;
			}

			if (string.IsNullOrEmpty(ViewTextBox.Text))
			{
				engine.Log("Script: Provisioning Spectrum Elements | Message: Validation Failed: View input string is null or empty");
				ViewTextBox.ValidationText = "Please Enter a View Name.";
				ViewTextBox.ValidationState = UIValidationState.Invalid;
				return false;
			}

			var existingView = dms.GetViews().FirstOrDefault(view => view.Name.Equals(ViewTextBox.Text, StringComparison.OrdinalIgnoreCase));
			if (existingView == null)
			{
				engine.Log("Script: Provisioning Spectrum Elements | Message: Validation Failed: View input string does not match an existing view");
				ViewTextBox.ValidationText = "View not found in the system.";
				ViewTextBox.ValidationState = UIValidationState.Invalid;
				return false;
			}

			ViewTextBox.ValidationText = string.Empty;
			ViewTextBox.ValidationState = UIValidationState.Valid;
			return true;
		}

		private bool ValidateLoadBalancedDma(IDms dms)
		{
			if (!LoadBalancedDmaCheckBox.IsChecked)
			{
				return true;
			}

			if (string.IsNullOrEmpty(LoadBalancedDmaTextBox.Text))
			{
				engine.Log("Script: Provisioning Spectrum Elements | Message: Validation Failed: DMA load input string is null or empty");
				LoadBalancedDmaTextBox.ValidationText = "Please Enter a Valid DMA Name.";
				LoadBalancedDmaTextBox.ValidationState = UIValidationState.Invalid;
				return false;
			}

			var dmaList = LoadBalancedDmaTextBox.Text.Split(',').Select(dma => dma.Trim()).ToList();
			var validDmas = dms.GetAgents().Select(agent => agent.HostName).ToHashSet(StringComparer.OrdinalIgnoreCase);

			var invalidDmas = dmaList.Where(dma => !validDmas.Contains(dma)).ToList();

			if (invalidDmas.Any())
			{
				engine.Log("Script: Provisioning Spectrum Elements | Message: Validation Failed: DMA input string does not match an existing DMA");
				LoadBalancedDmaTextBox.ValidationText = $"Invalid DMA(s): {string.Join(", ", invalidDmas)}";
				LoadBalancedDmaTextBox.ValidationState = UIValidationState.Invalid;
				return false;
			}

			LoadBalancedDmaTextBox.ValidationText = string.Empty;
			LoadBalancedDmaTextBox.ValidationState = UIValidationState.Valid;
			return true;
		}

		private void ProvisionButton_Pressed(object sender, System.EventArgs e)
		{
			if (!ValidateInput())
			{
				engine.GenerateInformation("Validation failed. Please correct the inputs and try again.");
				return;
			}

			CommunitySection.IsVisible = false;
			ViewSection.IsVisible = false;
			TargetDmaSection.IsVisible = false;
			ProvisionButton.IsVisible = false;
			CancelButton.IsVisible = false;
			ResultsPage.IsVisible = true;
			try
			{
				ProvisionSpectrumElements();
			}
			catch (Exception ex)
			{
				engine.GenerateInformation("An error occurred during provisioning: " + ex.Message);
			}
		}

		private void ProvisionSpectrumElements()
		{
			try
			{
				var dms = engine.GetDms();
				string getCommunity = GetCommunityText.Text;
				string setCommunity = SetCommunityText.Text;

				var allElements = dms.GetElements();
				var allElementnames = new List<string>();
				foreach(var element in allElements)
				{
					allElementnames.Add(element.Name);
				}

				var elements = allElements
								  .Where(x => x.Protocol.Name.Equals("CISCO CBR-8 CCAP Platform"))
								  .ToList();

				ExpectedElementsCreated.Text = $"Expected Elements to Create: {elements.Count}";

				var targetDmas = DetermineTargetDmas(dms);

				int dmaCount = targetDmas.Count;
				ResultBuilder = new StringBuilder();
				int elementsCreatedCounter = 0;

				// Distribute elements across DMAs
				for (int i = 0; i < elements.Count; i++)
				{
					IDma targetDma;
					var element = elements[i];
					targetDma = SameDmaCheckBox.IsChecked ? dms.GetAgent(element.AgentId) : targetDmas[i % dmaCount]; // Round-robin distribution

					string elementName = element.Name + "_utsc";
					if(allElementnames.Contains(elementName))
					{
						ResultBuilder.AppendLine($" - {elementName} already exists");
						continue;
					}

					// Assign element connection
					IDmsProtocol protocol = dms.GetProtocol("CISCO CBR-8 CCAP UTSC", "Production");

					var elementIP = element.Connections.OfType<ISnmpConnection>().FirstOrDefault()?.UdpConfiguration.RemoteHost;

					IUdp port = new Udp(elementIP, 161);

					ISnmpV2Connection snmpv2 = new SnmpV2Connection
					{
						UdpConfiguration = port,
						DeviceAddress = elementIP,
						GetCommunityString = getCommunity,
						SetCommunityString = setCommunity,
						Timeout = new TimeSpan(0, 0, 0, 120),
						Retries = 3,
						ElementTimeout = new TimeSpan(0, 0, 0, 120),
					};

					ElementConfiguration elementConfig = new ElementConfiguration(dms, elementName, protocol, new List<IElementConnection> { snmpv2 })
					{
						Description = "Spectrum Monitoring Element",
					};

					// Assign element state
					if (element.State.Equals(ElementStateIs.Active))
					{
						elementConfig.State = ConfigurationElementState.Active;
					}
					else if (element.State.Equals(ElementStateIs.Paused))
					{
						elementConfig.State = ConfigurationElementState.Paused;
					}
					else
					{
						elementConfig.State = ConfigurationElementState.Stopped;
					}

					// Assign element view
					string viewName = NewViewCheckBox.IsChecked ? ViewTextBox.Text : element.Views.First().Name;
					var targetView = dms.GetView(viewName);
					elementConfig.Views.Add(targetView);

					var newElementId = targetDma.CreateElement(elementConfig);

					var parts = Convert.ToString(newElementId).Split(new[] { "agent ID:", "element ID:" }, StringSplitOptions.RemoveEmptyEntries);

					if (parts.Length >= 2)
					{
						string agentId = parts[0].Trim().TrimEnd(',');
						string elementId = parts[1].Trim();
						string formattedId = $"{agentId}/{elementId}";
						ResultBuilder.AppendLine($" - Name: {elementName} ({formattedId})");
					}

					elementsCreatedCounter++;
				}

				ActualElementsCreated.Text = $"Successfully Created Elements: {elementsCreatedCounter}";
				Result.Text = ResultBuilder.ToString();
			}
			catch(Exception ex)
			{
				engine.Log(ex.Message);
			}
		}

		private List<IDma> DetermineTargetDmas(IDms dms)
		{
			var targetDmas = new List<IDma>();

			if (SpecifiedDmaCheckBox.IsChecked)
			{
				string selectedDma = SpecifiedDmaDropDown.Selected;
				targetDmas.Add(dms.GetAgents().FirstOrDefault(agent => agent.HostName == selectedDma));
			}

			if (LoadBalancedDmaCheckBox.IsChecked)
			{
				var dmaNames = LoadBalancedDmaTextBox.Text.Split(',').Select(dma => dma.Trim()).ToList();
				targetDmas = dms.GetAgents()
								.Where(agent => dmaNames.Contains(agent.HostName, StringComparer.OrdinalIgnoreCase))
								.ToList();
			}

			return targetDmas;
		}

		private void CancelButton_Pressed(object sender, System.EventArgs e)
		{
			engine.ExitSuccess("User canceled process");
		}

		private void ExitButton_Pressed(object sender, System.EventArgs e)
		{
			engine.ExitSuccess("User Successfully Exited Script");
		}
	}
}
