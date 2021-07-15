using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace SampleWebApplication.ServiceModel
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFeed" in both code and config file together.
    [ServiceContract]
    public interface IFeed
    {
        [OperationContract(Action = "GetFeed")]
        Task<string> GetFeedAsync();
    }
}
