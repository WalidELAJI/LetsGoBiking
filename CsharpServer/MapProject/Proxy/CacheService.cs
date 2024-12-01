using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapProject.Proxy
{
    public class CacheService<T>
    {

        // Dictionnaire pour stocker les données avec leur expiration
        private readonly ConcurrentDictionary<string, (T Value, DateTime Expiration)> _cache;

        // Temps d'expiration par défaut
        private readonly TimeSpan _defaultExpiration;

        public CacheService(TimeSpan defaultExpiration)
        {
            _defaultExpiration = defaultExpiration;
            _cache = new ConcurrentDictionary<string, (T Value, DateTime Expiration)>();
        }

        /// <summary>
        /// Récupère une valeur du cache de manière asynchrone ou l'ajoute si elle est absente ou expirée.
        /// </summary>
        /// <param name="key">Clé unique pour identifier la donnée.</param>
        /// <param name="fetchAsync">Fonction asynchrone pour récupérer la donnée si elle est absente ou expirée.</param>
        /// <returns>La valeur récupérée ou mise à jour dans le cache.</returns>
        public async Task<T> GetOrAddAsync(string key, Func<Task<T>> fetchAsync)
        {
            // Vérifie si la clé existe dans le cache
            if (_cache.TryGetValue(key, out var entry))
            {
                // Si la valeur est encore valide, la retourner
                if (entry.Expiration > DateTime.UtcNow)
                {
                    return entry.Value;
                }
            }

            // Si la valeur n'est pas valide ou absente, la récupérer via la fonction asynchrone
            var newValue = await fetchAsync();

            // Calculer la nouvelle expiration et stocker dans le cache
            var newExpiration = DateTime.UtcNow.Add(_defaultExpiration);
            _cache[key] = (newValue, newExpiration);

            return newValue;
        }

        /// <summary>
        /// Supprime une entrée spécifique du cache.
        /// </summary>
        /// <param name="key">La clé de l'élément à supprimer.</param>
        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// Efface toutes les entrées du cache.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }
    }
}

