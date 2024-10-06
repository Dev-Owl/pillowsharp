using System.Linq;
using System.Collections.Generic;
using PillowSharp.CouchType;

namespace PillowSharp.Helper
{
    public static class MangoQueryHelper
    {
        public static MangoSelectorOperator ImplicitEquals(string field, object value)
        {
            return new MangoSelectorOperator(field)
            {
                SimpleOperatorValue = value
            };
        }

        public static MangoSelectorOperator Or(params MangoSelectorOperator[] operators)
        {
            return new MangoSelectorOperator("$or")
            {
                OperatorValues = operators.ToList()
            };
        }
        public static MangoSelectorOperator And(params MangoSelectorOperator[] operators)
        {
            return new MangoSelectorOperator("$and")
            {
                OperatorValues = operators.ToList()
            };
        }

        public static MangoSelectorOperator ValueIsEqual(string fieldName, object value)
        {
            return new MangoSelectorOperator(fieldName)
            {
                OperatorValues = new List<MangoSelectorOperator>(){
                            new MangoSelectorOperator("$eq"){
                                SimpleOperatorValue = value
                            }
                        }
            };
        }

        public static MangoQuery FieldIsEqualQuery(string fieldName, object value)
        {
            return BasicQuery(ValueIsEqual(fieldName, value));
        }

        public static MangoQuery BasicQuery(params MangoSelectorOperator[] selectorOperators)
        {
            return new MangoQuery()
            {
                Selector = new MangoSelector()
                {
                    Operations = selectorOperators.ToList()
                }
            };
        }

    }
}
