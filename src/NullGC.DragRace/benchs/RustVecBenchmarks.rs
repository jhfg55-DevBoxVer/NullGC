use criterion::{black_box, criterion_group, criterion_main, Criterion};

fn add_elements_benchmark(c: &mut Criterion) {
    let count: usize = 1_000_000;
    c.bench_function("vec_add_elements", |b| {
        b.iter(|| {
            let mut vec = Vec::with_capacity(0);
            for i in 0..count {
                vec.push(i);
            }
            // Prevent compiler optimizations.
            black_box(vec);
        })
    });
}

fn clear_vec_benchmark(c: &mut Criterion) {
    let count: usize = 1_000_000;
    // 预先创建一个包含 count 个元素的向量
    c.bench_function("vec_clear", |b| {
        let mut vec = Vec::with_capacity(count);
        for i in 0..count {
            vec.push(i);
        }
        b.iter(|| {
            vec.clear();
            // 确保 clear 后的向量被使用，以防编译器优化掉 clear 操作
            black_box(&vec);
            // 重新填充向量供下次迭代使用
            for i in 0..count {
                vec.push(i);
            }
        })
    });
}

criterion_group!(benches, add_elements_benchmark, clear_vec_benchmark);
criterion_main!(benches);
