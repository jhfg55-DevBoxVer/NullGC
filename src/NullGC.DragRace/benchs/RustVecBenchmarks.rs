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
    // Ԥ�ȴ���һ������ count ��Ԫ�ص�����
    c.bench_function("vec_clear", |b| {
        let mut vec = Vec::with_capacity(count);
        for i in 0..count {
            vec.push(i);
        }
        b.iter(|| {
            vec.clear();
            // ȷ�� clear ���������ʹ�ã��Է��������Ż��� clear ����
            black_box(&vec);
            // ��������������´ε���ʹ��
            for i in 0..count {
                vec.push(i);
            }
        })
    });
}

criterion_group!(benches, add_elements_benchmark, clear_vec_benchmark);
criterion_main!(benches);
