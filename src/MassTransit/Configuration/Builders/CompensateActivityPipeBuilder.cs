﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Builders
{
    using System;
    using System.Collections.Generic;
    using Courier;
    using Courier.Pipeline;
    using GreenPipes;
    using Pipeline.Filters;


    public static class CompensateActivityPipeBuilder
    {
        public static IPipe<CompensateActivityContext<TActivity, TLog>> Build<TActivity, TLog>(
            this IEnumerable<IPipeSpecification<CompensateActivityContext<TActivity, TLog>>> pipeSpecifications,
            IFilter<CompensateActivityContext<TActivity, TLog>> consumeFilter)
            where TActivity : class, CompensateActivity<TLog>
            where TLog : class
        {
            var builder = new CompensateActivityPipeBuilder<TActivity, TLog>();
            foreach (IPipeSpecification<CompensateActivityContext<TActivity, TLog>> specification in pipeSpecifications)
                specification.Apply(builder);

            CompensateActivityPipeBuilder<TActivity, TLog> builders = builder;
            if (builders == null)
                throw new InvalidOperationException("Should not be null, ever");

            return Pipe.New<CompensateActivityContext<TActivity, TLog>>(x =>
            {
                foreach (IFilter<CompensateActivityContext<TActivity, TLog>> filter in builders.Filters)
                    x.UseFilter(filter);

                x.UseFilter(consumeFilter);
            });
        }
    }


    public class CompensateActivityPipeBuilder<TActivity, TLog> :
        IPipeBuilder<CompensateActivityContext<TLog>>,
        IPipeBuilder<CompensateActivityContext<TActivity, TLog>>
        where TActivity : class, CompensateActivity<TLog>
        where TLog : class
    {
        readonly IList<IFilter<CompensateActivityContext<TActivity, TLog>>> _filters;

        public CompensateActivityPipeBuilder()
        {
            _filters = new List<IFilter<CompensateActivityContext<TActivity, TLog>>>();
        }

        public IEnumerable<IFilter<CompensateActivityContext<TActivity, TLog>>> Filters => _filters;

        public void AddFilter(IFilter<CompensateActivityContext<TActivity, TLog>> filter)
        {
            _filters.Add(filter);
        }

        public void AddFilter(IFilter<CompensateActivityContext<TLog>> filter)
        {
            _filters.Add(new CompensateActivitySplitFilter<TActivity, TLog>(filter));
        }
    }
}