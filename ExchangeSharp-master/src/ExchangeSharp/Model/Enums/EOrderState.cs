/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace ExchangeSharp
{
    /// <summary>Result of exchange order</summary>
    public enum EOrderState
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        Unknown,

        /// <summary>
        /// 下单中
        /// </summary>
        PendingNew,

        /// <summary>
        /// 已下单
        /// </summary>
        New,

        /// <summary>
        /// 预提交（埋单）
        /// </summary>
        PreSubmited,

        /// <summary>
        /// 撤单中
        /// </summary>
        PendingCancel,

        /// <summary>
        /// 部成
        /// </summary>
        PartiallyFilled,

        /// <summary>
        /// 全成
        /// </summary>
        Filled,

        /// <summary>
        /// 废单
        /// </summary>
        Rejected,

        /// <summary>
        /// 全撤
        /// </summary>
        Canceled,

        /// <summary>
        /// 部撤
        /// </summary>
        PartiallyCanceled,

        /// <summary>
        /// 假定已完成（查询时不明确时使用，作为终态）
        /// </summary>
        PendingFinished
    }
}