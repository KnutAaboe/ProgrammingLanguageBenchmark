mod utils;

use wasm_bindgen::prelude::*;

// When the `wee_alloc` feature is enabled, use `wee_alloc` as the global
// allocator.
#[cfg(feature = "wee_alloc")]
#[global_allocator]
static ALLOC: wee_alloc::WeeAlloc = wee_alloc::WeeAlloc::INIT;

#[wasm_bindgen]
pub fn in_loop() {
    let (mut x, mut y) = (0, 0);
    
    for _n in 1..10000000 {
        x = 1 + 1;
        y = y + x;
    }
}

#[wasm_bindgen]
pub fn out_loop() {
    let (x, mut y) = (1 + 1, 0);
    
    for _n in 1..10000000 {
        y = y + x;
    }
}