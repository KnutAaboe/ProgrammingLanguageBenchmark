// The Computer Language Benchmarks Game
// http://benchmarksgame.alioth.debian.org/
//
// contributed by the Rust Project Developers
// contributed by TeXitoi
// multi-threaded version contributed by Alisdair Owens

extern crate num_cpus;
extern crate console_error_panic_hook;

use std::cmp::min;
use std::io;
use std::io::{Write, BufWriter};
use std::sync::{Mutex,Arc};
use wasm_bindgen::prelude::*;
use wasm_bindgen::JsValue;

const LINE_LENGTH: usize = 60;
const IM: u32 = 139968;
const LINES: usize = 1024;
const BLKLEN: usize = LINE_LENGTH * LINES;

struct MyStdOut {
    stdout: io::Stdout,
}

struct MyRandom {
    last: u32,
    count: usize,
}

impl MyRandom {
    fn new(count: usize) -> MyRandom {
        MyRandom { 
            last: 42,
            count: count,
        }
    }
    
    fn normalize(p: f32) -> u32 {(p * IM as f32).floor() as u32}

    fn reset(&mut self, count: usize) {
        self.count = count;
    }

    fn gen(&mut self, buf: &mut [u32]) -> Result<usize,()> {

        let to_gen = min(buf.len(), self.count);
        for i in 0..to_gen {
            self.last = (self.last * 3877 + 29573) % IM;
            buf[i] = self.last;
        }
        self.count -= to_gen;
        Ok(to_gen)
    }
}

impl MyStdOut {
    fn new() -> MyStdOut {
        MyStdOut {
            stdout: io::stdout() 
        } 
    }
    fn write(&mut self, data: &[u8]) -> io::Result<()> {
        self.stdout.write_all(data)
    }
}

fn make_random(data: &[(char, f32)]) -> Vec<(u32, u8)> {
    let mut acc = 0.;
    data.iter()
        .map(|&(ch, p)| {
            acc += p;
            (MyRandom::normalize(acc), ch as u8)
        })
        .collect()
}

fn make_fasta2<I: Iterator<Item=u8>>(header: &str, mut it: I, mut n: usize)
    -> io::Result<()> {
    let mut sysout = BufWriter::new(io::stdout());
    (sysout.write_all(header.as_bytes()))?;
    let mut line = [0u8; LINE_LENGTH + 1];
    while n > 0 {
        let nb = min(LINE_LENGTH, n);
        for i in 0..nb {
            line[i] = it.next().unwrap();
        }
        n -= nb;
        line[nb] = '\n' as u8;
        (sysout.write_all(&line[..(nb+1)]))?;
    }
    Ok(())
}

fn do_fasta(rng: Arc<Mutex<MyRandom>>,
            wr: Arc<Mutex<MyStdOut>>, data: Vec<(u32, u8)>) {
    
    let mut rng_buf = [0u32; BLKLEN];
    let mut out_buf = [0u8; BLKLEN + LINES];
    let mut count;
	
    loop {
        count = match rng.lock().unwrap().gen(&mut rng_buf[..]) {
            Ok(x) => x,
            Err(_) => break,
        };

        if count == 0 {
            break;
        }
        let mut line_count = 0;
        for i in 0..count {
            let r = rng_buf[i];
            //let p = (r & 15) as f32 * 0.1;

            if line_count == LINE_LENGTH {
                out_buf[line_count] = b'\n';
                line_count += 1;
                wr.lock().unwrap().write(&out_buf[..line_count]).unwrap();
                line_count = 0;
            }

            let d = &data;

            let c = match d.binary_search_by(|p| p.0.cmp(&r)) {
                Ok(idx) => d[idx].1,
                Err(idx) => {
                    if idx == 0 {
                        d[idx].1
                    } else {
                        d[idx - 1].1
                    }
                }
            };

            out_buf[line_count] = c;
            line_count += 1;
        }

        if line_count > 0 {
            out_buf[line_count] = b'\n';
            line_count += 1;
            wr.lock().unwrap().write(&out_buf[..line_count]).unwrap();
        }
    }
}

fn make_fasta(header: &str, rng: Arc<Mutex<MyRandom>>,
                 data: Vec<(u32, u8)>
             ) -> io::Result<()> {

    let stdout = MyStdOut::new();
    io::stdout().write_all(header.as_bytes())?;
    do_fasta(rng.clone(), Arc::new(Mutex::new(stdout)), data);
    Ok(())
}


#[wasm_bindgen]
pub fn main_js() {
	console_error_panic_hook::set_once();
	
    if let Err(err) = run_main_js() {
        let error_message = format!("Error: {}", err);
        let js_error = JsValue::from_str(&error_message);
        console_error(&js_error);
        panic!();
    }
}

#[wasm_bindgen]
extern "C" {
    #[wasm_bindgen(js_namespace = console)]
    fn error(s: &str);
}

fn console_error(js_value: &JsValue) {
    if let Some(s) = js_value.as_string() {
        error(&s);
    } else {
        error("Unknown error");
    }
}


fn run_main_js() -> Result<(), Box<dyn std::error::Error>> {
    let n = 1000; //TODO input

    let rng = Arc::new(Mutex::new(MyRandom::new(n*3)));
    let alu: &[u8] = b"GGCCGGGCGCGGTGGCTCACGCCTGTAATCCCAGCACTTT\
                       GGGAGGCCGAGGCGGGCGGATCACCTGAGGTCAGGAGTTC\
                       GAGACCAGCCTGGCCAACATGGTGAAACCCCGTCTCTACT\
                       AAAAATACAAAAATTAGCCGGGCGTGGTGGCGCGCGCCTG\
                       TAATCCCAGCTACTCGGGAGGCTGAGGCAGGAGAATCGCT\
                       TGAACCCGGGAGGCGGAGGTTGCAGTGAGCCGAGATCGCG\
                       CCACTGCACTCCAGCCTGGGCGACAGAGCGAGACTCCGTCT\
                       CAAAAA";

    let iub = &[('a', 0.27), ('c', 0.12), ('g', 0.12),
                ('t', 0.27), ('B', 0.02), ('D', 0.02),
                ('H', 0.02), ('K', 0.02), ('M', 0.02),
                ('N', 0.02), ('R', 0.02), ('S', 0.02),
                ('V', 0.02), ('W', 0.02), ('Y', 0.02)];

    let homosapiens = &[('a', 0.3029549426680),
                        ('c', 0.1979883004921),
                        ('g', 0.1975473066391),
                        ('t', 0.3015094502008)];

    make_fasta2(">ONE Homo sapiens alu\n",
                    alu.iter().cycle().map(|c| *c), n * 2)?;
    make_fasta(">TWO IUB ambiguity codes\n",
                    rng.clone(), make_random(iub))?;

    rng.lock().unwrap().reset(n*5);

    make_fasta(">THREE Homo sapiens frequency\n",
                    rng, make_random(homosapiens))?;

    Ok(())
}