using System;
using System.Collections.Generic;
using UnityEngine;


namespace Services {
    public static class ServiceLocator {
        private static readonly List<UnityEngine.Object> _services = new List<UnityEngine.Object>();



        public static void Subscribe( UnityEngine.Object pService ) {
            _services.Add( pService );
            Console.WriteLine( $"Servive of type {pService.GetType()} subscribed." );
        }

        public static void UnSubscribe( UnityEngine.Object pService ) {
            _services.Remove( pService );
        }


        public static UnityEngine.Object[] RetrieveByType( Type pTypeOfService ) {
            List<UnityEngine.Object> servicesFound = new List<UnityEngine.Object>();

            foreach ( UnityEngine.Object service in _services ) {
                if ( service.GetType().Equals( pTypeOfService ) ) {
                    servicesFound.Add( service );
                }
            }

            return servicesFound.ToArray();
        }


        public static UnityEngine.Object[] RetrieveByTag( string pTag ) {
            List<UnityEngine.Object> servicesFound = new List<UnityEngine.Object>();

            foreach ( UnityEngine.Object service in _services ) {
                if ( service is GameObject && ( ( GameObject )service ).tag.Equals( pTag ) ) {
                    servicesFound.Add( service );
                }
            }

            return servicesFound.ToArray();
        }


        public static UnityEngine.Object[] RetrieveByName( string pName ) {
            List<UnityEngine.Object> servicesFound = new List<UnityEngine.Object>();

            foreach ( UnityEngine.Object service in _services ) {
                if ( service.name.Equals( pName ) ) {
                    servicesFound.Add( service );
                }
            }

            return servicesFound.ToArray();
        }
    }
}