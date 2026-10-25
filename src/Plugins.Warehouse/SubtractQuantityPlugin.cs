using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using TALXIS.DevKit.Build.Dataverse.Plugin;

namespace Plugins.Warehouse
{
    [PluginStep("Create", "lab_warehousetransaction", PluginStage.PostOperation)]
    public class SubtractQuantityPlugin : PluginBase
    {
        public SubtractQuantityPlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(SubtractQuantityPlugin))
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target) || target.LogicalName != "lab_warehousetransaction")
            {
                return;
            }

            if (!target.Contains("lab_quantity") || !target.Contains("lab_itemid") || !target.Contains("lab_transactiontype"))
            {
                return;
            }

            if (!(target["lab_itemid"] is EntityReference itemRef))
            {
                return;
            }

            if (!(target["lab_transactiontype"] is OptionSetValue transactionType))
            {
                return;
            }

            var quantity = (int)target["lab_quantity"];
            var item = service.Retrieve("lab_warehouseitem", itemRef.Id, new ColumnSet("lab_availablequantity"));
            var available = item != null && item.Contains("lab_availablequantity") ? (int)item["lab_availablequantity"] : 0;

            switch (transactionType.Value)
            {
                case 100000000:
                    item["lab_availablequantity"] = available + quantity;
                    break;
                case 100000001:
                    item["lab_availablequantity"] = available - quantity;
                    break;
                default:
                    return;
            }

            service.Update(item);
        }
    }
}
