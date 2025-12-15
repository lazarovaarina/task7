using System;
using System.Collections.Generic;
using System.IO;

public class MyPriorityQueue<T>
{
    private T[] queue;
    private int size;
    private readonly IComparer<T> comparer;

    public MyPriorityQueue() : this(11, Comparer<T>.Default) { }

    public MyPriorityQueue(T[] a) : this(a.Length, Comparer<T>.Default)
    {
        foreach (var item in a) Add(item);
    }

    public MyPriorityQueue(int initialCapacity) : this(initialCapacity, Comparer<T>.Default) { }

    public MyPriorityQueue(int initialCapacity, IComparer<T> comparator)
    {
        if (initialCapacity < 1) throw new ArgumentException("Capacity must be at least 1");
        queue = new T[initialCapacity];
        this.comparer = comparator ?? Comparer<T>.Default;
    }

    public MyPriorityQueue(MyPriorityQueue<T> c) : this(c?.size ?? 0, c?.comparer)
    {
        if (c == null) throw new ArgumentNullException(nameof(c));
        Array.Copy(c.queue, queue, c.size);
        size = c.size;
    }

    public void Add(T e)
    {
        if (size == queue.Length) Resize();
        queue[size] = e;
        SiftUp(size++);
    }

    public void AddAll(T[] a)
    {
        foreach (var item in a) Add(item);
    }

    public void Clear()
    {
        Array.Clear(queue, 0, size);
        size = 0;
    }

    public bool Contains(object o)
    {
        if (o is T item)
        {
            for (int i = 0; i < size; i++)
                if (EqualityComparer<T>.Default.Equals(queue[i], item))
                    return true;
        }
        return false;
    }

    public bool ContainsAll(T[] a)
    {
        foreach (var item in a)
            if (!Contains(item)) return false;
        return true;
    }

    public bool IsEmpty() => size == 0;

    public bool Remove(object o)
    {
        if (!(o is T item)) return false;
        int index = Array.IndexOf(queue, item);
        if (index < 0) return false;
        queue[index] = queue[--size];
        SiftDown(index);
        return true;
    }

    public void RemoveAll(T[] a)
    {
        foreach (var item in a) Remove(item);
    }

    public void RetainAll(T[] a)
    {
        var set = new HashSet<T>(a);
        var newQueue = new T[queue.Length];
        int newSize = 0;
        for (int i = 0; i < size; i++)
        {
            if (set.Contains(queue[i]))
                newQueue[newSize++] = queue[i];
        }
        queue = newQueue;
        size = newSize;
        Heapify();
    }

    public int Size() => size;

    public T[] ToArray()
    {
        var result = new T[size];
        Array.Copy(queue, result, size);
        return result;
    }

    public T[] ToArray(T[] a)
    {
        if (a == null) return ToArray();
        if (a.Length < size) return ToArray();
        Array.Copy(queue, a, size);
        return a;
    }

    public T Element()
    {
        if (size == 0) throw new InvalidOperationException("Queue is empty");
        return queue[0];
    }

    public bool Offer(T obj)
    {
        Add(obj);
        return true;
    }

    public T Peek() => size == 0 ? default : queue[0];

    public T Poll()
    {
        if (size == 0) return default;
        T result = queue[0];
        queue[0] = queue[--size];
        SiftDown(0);
        return result;
    }

    private void Resize()
    {
        int newCapacity = queue.Length < 64 ? queue.Length * 2 : (int)(queue.Length * 1.5);
        Array.Resize(ref queue, newCapacity);
    }

    private void SiftUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (comparer.Compare(queue[index], queue[parent]) >= 0) break;
            Swap(index, parent);
            index = parent;
        }
    }

    private void SiftDown(int index)
    {
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int smallest = index;

            if (left < size && comparer.Compare(queue[left], queue[smallest]) < 0)
                smallest = left;
            if (right < size && comparer.Compare(queue[right], queue[smallest]) < 0)
                smallest = right;

            if (smallest == index) break;
            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Heapify()
    {
        for (int i = size / 2 - 1; i >= 0; i--)
            SiftDown(i);
    }

    private void Swap(int i, int j)
    {
        T temp = queue[i];
        queue[i] = queue[j];
        queue[j] = temp;
    }
}

public class Request : IComparable<Request>
{
    public int Priority { get; }
    public int RequestId { get; }
    public int StepAdded { get; }
    public int StepRemoved { get; set; }
    public int WaitTime => StepRemoved - StepAdded;

    public Request(int priority, int requestId, int stepAdded)
    {
        Priority = priority;
        RequestId = requestId;
        StepAdded = stepAdded;
    }

    // Сравнение: более высокий приоритет = большее значение
    public int CompareTo(Request other)
    {
        if (other == null) return 1;
        return other.Priority.CompareTo(Priority); 
    }
}

class Program
{
    static void Main()
    {
        StreamWriter logFile = null;

        try
        {
            logFile = new StreamWriter("C:/Users/lazar/Desktop/log.txt");

            Console.WriteLine("ПРИОРИТЕТНАЯ ОЧЕРЕДЬ ЗАЯВОК");
            Console.Write("Введите количество шагов N: ");

            string input = Console.ReadLine();
            if (!int.TryParse(input, out int N) || N <= 0)
            {
                Console.WriteLine("Ошибка: N должно быть положительным целым числом!");
                return;
            }

            var queue = new MyPriorityQueue<Request>();
            var random = new Random();
            int requestCounter = 0;
            Request maxWaitRequest = null;

            Console.WriteLine($"\nНачинаем обработку {N} шагов");

            for (int step = 1; step <= N; step++)
            {
                Console.WriteLine($"\nШаг {step}");

                int requestsToAdd = random.Next(1, 11);
                Console.WriteLine($"Добавляем {requestsToAdd} заявок:");

                for (int i = 0; i < requestsToAdd; i++)
                {
                    requestCounter++;
                    int priority = random.Next(1, 6);
                    var request = new Request(priority, requestCounter, step);

                    queue.Add(request);

                    logFile.WriteLine($"ADD {request.RequestId} {request.Priority} {request.StepAdded}");

                    Console.WriteLine($"  #{request.RequestId} - приоритет: {priority}");
                }

                Console.WriteLine($"Всего заявок в очереди: {queue.Size()}");

                if (!queue.IsEmpty())
                {
                    var removedRequest = queue.Poll();
                    removedRequest.StepRemoved = step;

                    logFile.WriteLine($"REMOVE {removedRequest.RequestId} {removedRequest.Priority} {step}");

                    Console.WriteLine($"Удалена заявка #{removedRequest.RequestId} " + $"(приоритет: {removedRequest.Priority}, " + $"время ожидания: {removedRequest.WaitTime} шагов)");

                    if (maxWaitRequest == null || removedRequest.WaitTime > maxWaitRequest.WaitTime)
                    {
                        maxWaitRequest = removedRequest;
                        Console.WriteLine($"Новый максимум времени ожидания: {maxWaitRequest.WaitTime} шагов");
                    }
                }
                else
                {
                    Console.WriteLine("Очередь пуста, удалять нечего");
                }
            }

            Console.WriteLine($"\nЗавершено {N} шагов с генерацией");
            Console.WriteLine("Переходим к удалению оставшихся заявок");

            int removeStep = N + 1;
            int removedCount = 0;

            while (!queue.IsEmpty())
            {
                Console.WriteLine($"\nШаг {removeStep} (без генерации)");

                var removedRequest = queue.Poll();
                removedRequest.StepRemoved = removeStep;
                removedCount++;

                logFile.WriteLine($"REMOVE {removedRequest.RequestId} {removedRequest.Priority} {removeStep}");

                Console.WriteLine($"Удалена заявка #{removedRequest.RequestId} " +  $"(приоритет: {removedRequest.Priority}, " + $"время ожидания: {removedRequest.WaitTime} шагов)");

                if (maxWaitRequest == null || removedRequest.WaitTime > maxWaitRequest.WaitTime)
                {
                    maxWaitRequest = removedRequest;
                    Console.WriteLine($"Новый максимум времени ожидания: {maxWaitRequest.WaitTime} шагов");
                }

                Console.WriteLine($"Осталось заявок в очереди: {queue.Size()}");
                removeStep++;
            }

            Console.WriteLine($"\nОбработка завершена");
            Console.WriteLine($"Всего было создано заявок: {requestCounter}");
            Console.WriteLine($"Удалено на этапе без генерации: {removedCount}");

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("РЕЗУЛЬТАТЫ:");
            Console.WriteLine(new string('=', 50));

            if (maxWaitRequest != null)
            {
                Console.WriteLine($"Максимальное время ожидания: {maxWaitRequest.WaitTime} шагов");
                Console.WriteLine("\nИнформация о заявке с максимальным временем ожидания:");
                Console.WriteLine($"  Номер заявки: #{maxWaitRequest.RequestId}");
                Console.WriteLine($"  Приоритет: {maxWaitRequest.Priority}");
                Console.WriteLine($"  Добавлена на шаге: {maxWaitRequest.StepAdded}");
                Console.WriteLine($"  Удалена на шаге: {maxWaitRequest.StepRemoved}");
                Console.WriteLine($"  Время ожидания: {maxWaitRequest.WaitTime} шагов");
            }
            else
            {
                Console.WriteLine("Заявок не было обработано");
            }

            Console.WriteLine($"\nЛоги записаны в файл: log.txt");
            Console.WriteLine($"Размер файла лога: {new FileInfo("log.txt").Length} байт");
        }
        catch (FormatException)
        {
            Console.WriteLine("Ошибка: введено некорректное число!");
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"Ошибка ввода-вывода: {ioEx.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Ошибка: нет прав для записи в файл log.txt!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
            Console.WriteLine($"Тип ошибки: {ex.GetType().Name}");
        }
        finally
        {
            try
            {
                logFile?.Close();
                Console.WriteLine("\nРесурсы освобождены.");
            }
            catch (Exception closeEx)
            {
                Console.WriteLine($"Ошибка при закрытии файла: {closeEx.Message}");
            }
        }

        Console.WriteLine("\nНажмите любую клавишу для выхода");
        Console.ReadKey();
    }
}