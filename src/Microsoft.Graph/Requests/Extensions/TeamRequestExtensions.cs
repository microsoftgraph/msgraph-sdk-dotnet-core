using System.Threading;

namespace Microsoft.Graph
{
    public partial interface ITeamRequest
    {
        /// <summary>
        /// Creates the specified Team using PUT.
        /// </summary>
        /// <param name="teamToCreate">The Team to create.</param>
        /// <returns>The created Team.</returns>
        System.Threading.Tasks.Task<Team> PutAsync(Team teamToCreate);        
        
        /// <summary>
        /// Creates the specified Team using PUT.
        /// </summary>
        /// <param name="teamToCreate">The Team to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created Team.</returns>
        System.Threading.Tasks.Task<Team> PutAsync(Team teamToCreate, CancellationToken cancellationToken);
    }

    public partial class TeamRequest
    {
        /// <summary>
        /// Creates the specified Team using PUT.
        /// </summary>
        /// <param name="teamToCreate">The Team to create.</param>
        /// <returns>The created Team.</returns>
        public System.Threading.Tasks.Task<Team> PutAsync(Team teamToCreate)
        {
            return this.PutAsync(teamToCreate, CancellationToken.None);
        }

        /// <summary>
        /// Creates the specified Team using PUT.
        /// </summary>
        /// <param name="teamToCreate">The Team to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created Team.</returns>
        public async System.Threading.Tasks.Task<Team> PutAsync(Team teamToCreate, CancellationToken cancellationToken)
        {
            this.ContentType = "application/json";
            this.Method = "PUT";
            var newEntity = await this.SendAsync<Team>(teamToCreate, cancellationToken).ConfigureAwait(false);
            this.InitializeCollectionProperties(newEntity);
            return newEntity;
        }
    }
}