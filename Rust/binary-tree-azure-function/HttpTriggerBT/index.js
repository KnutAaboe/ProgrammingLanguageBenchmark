let binarytree = require("binary-tree-rust");

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');
    console.log(binarytree);
    console.log(binarytree.__wasm);
    console.log("---------- generate tree ----------")
    try{
        let treeRes = binarytree.generateTree();
        console.log("tree:", treeRes);
        context.res = {
            status: 200, /* Defaults to 200 */
            body: treeRes
        };
    } catch (error) {
        console.error(error);
    }
}