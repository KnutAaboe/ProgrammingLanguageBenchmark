require 'uri'
require 'net/http'

["HttpTriggerF", "HttpTriggerBT", "HttpTriggerFR", "HttpTriggerS"].each do |function|
    puts "Starting runs for #{function}"
    uri = URI("#{ARGV[0]}/api/#{function}")
    5.times do
        puts "requesting uri #{uri}"
        res = Net::HTTP.get(uri)
        # puts "#{ARGV[0]}/api/#{function}"
        pp res
        sleep 0.1
    end

    sleep 1

end

puts "all done"