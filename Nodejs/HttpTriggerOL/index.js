function out_loop(n) {
    let x = 1 +1, y = 0;

    for (var i = 0; i < n; i++) {
        y = y + x;
    }
}

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');
    out_loop(10000000);
};