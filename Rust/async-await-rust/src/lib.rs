mod utils;

use wasm_bindgen::prelude::*;
use wasm_bindgen_futures;

// When the `wee_alloc` feature is enabled, use `wee_alloc` as the global
// allocator.
#[cfg(feature = "wee_alloc")]
#[global_allocator]
static ALLOC: wee_alloc::WeeAlloc = wee_alloc::WeeAlloc::INIT;

#[wasm_bindgen]
pub async fn async_fn() {
    let mut _count = 0;
    for _n in 1..10000000 {
        _count += unnecessary_async().await;
    }
}

#[wasm_bindgen]
pub fn sync_fn() {
    let mut _count = 0;
    for _n in 1..10000000 {
        _count += not_async();
    }
}

async fn unnecessary_async() -> u8 {
    return 1;
}

fn not_async() -> u8 {
    return 1;
}