﻿using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Devon4Net.Infrastructure.AWS.CDK.Interfaces;

namespace Devon4Net.Infrastructure.AWS.CDK.Handlers
{
    public class AwsCdkVpcHandler : AwsCdkDefaultHandler, IAwsVpcHandler
    {
        public AwsCdkVpcHandler(Construct scope, string applicationName, string environmentName) : base(scope, applicationName, environmentName)
        {
        }

        public IVpc Locate(string identification, string vpcId, bool isDefault = true)
        {
            if (string.IsNullOrEmpty(vpcId))
            {
                throw new ArgumentException("The VPC id cannot be null ");
            }

            var result = Vpc.FromLookup(Scope, string.IsNullOrEmpty(identification) ? $"{ApplicationName}{EnvironmentName}vpc" : identification, new VpcLookupOptions
            {
                IsDefault = isDefault,
                VpcId = vpcId
            });

            if (result == null)
            {
                throw new ArgumentException($"The provided vpcId {vpcId} does not exists");
            }

            return result;
        }

        public IVpc Create(string identification, string cidr, double? maxAzs, DefaultInstanceTenancy defaultInstanceTenancy = DefaultInstanceTenancy.DEFAULT, bool enableDnsSupport = true, bool enableDnsHostnames = true, List<ISubnetConfiguration> subnetConfigurations = null, Dictionary<string, string> tags = null)
        {
            var vpc = new Vpc(Scope, identification, new VpcProps
            {
                Cidr = cidr,
                MaxAzs =maxAzs,
                DefaultInstanceTenancy = defaultInstanceTenancy,
                EnableDnsSupport = enableDnsSupport,
                EnableDnsHostnames = enableDnsHostnames,
                SubnetConfiguration = subnetConfigurations?.ToArray()
            });

            if (tags == null) return vpc;

            foreach (var (key, value) in tags)
            {
                Tags.Of(vpc).Add(key, value);
            }

            return vpc;
        }

        public ISubnetSelection GetVpcSubnetSelection(IVpc vpc, string subnetDomainToCheck, string defaultSubnetDomainSeparator = ",", SubnetType defaultSubnetType = SubnetType.PRIVATE)
        {
            if (string.IsNullOrEmpty(subnetDomainToCheck)) return new SubnetSelection {SubnetType = defaultSubnetType};

            var subnetIds = subnetDomainToCheck.Split(defaultSubnetDomainSeparator).ToList();
            return new SubnetSelection { Subnets = vpc.PrivateSubnets.Where(x => subnetIds.Contains(x.SubnetId)).ToArray() };
        }
    }
}