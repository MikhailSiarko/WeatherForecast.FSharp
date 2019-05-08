namespace Microsoft.FSharp.Quotations

module Expr =
    open System
    open System.Linq.Expressions
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Linq.RuntimeHelpers

    let ToFuncExpression (expr:Expr<'a -> 'b>) =
        let call = LeafExpressionConverter.QuotationToExpression expr :?> MethodCallExpression
        let lambda = call.Arguments.[0] :?> LambdaExpression
        Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters) 

namespace AutoMapper

[<AutoOpen>]
module AutoMap =
    open Microsoft.FSharp.Quotations

    let forMember (destMember: Expr<'dest -> 'mbr>) 
                  (memberOpts: IMemberConfigurationExpression<'source, 'dest, _> -> unit) 
                  (map: IMappingExpression<'source, 'dest>) =
        map.ForMember(Expr.ToFuncExpression destMember, memberOpts)

    let mapMember destMember (sourceMap:Expr<'source -> 'mapped>) =
        forMember destMember (fun o -> o.MapFrom<'mapped>(Expr.ToFuncExpression sourceMap))

    let ignoreMember destMember =
        forMember destMember (fun o -> o.Ignore())

    let forMemberName (destMember: string) 
                      (memberOpts: IMemberConfigurationExpression<'source, 'dest, _> -> unit) 
                      (map: IMappingExpression<'source, 'dest>) =
        map.ForMember(destMember, memberOpts)

    let mapMemberName destMember (sourceMap:Expr<'source -> 'mapped>) =
        forMemberName destMember (fun o -> o.MapFrom<'mapped>(Expr.ToFuncExpression sourceMap))

    let ignoreMemberName destMember =
        forMemberName destMember (fun o -> o.Ignore())
        
    let mapTo<'dest> source = Mapper.Map<_, 'dest>(source)